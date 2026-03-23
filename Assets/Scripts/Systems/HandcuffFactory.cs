using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.NPC;
using PlayableAd.Player;
using PlayableAd.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 광물 반납존(OreDepositZone)에서 광물을 받아 수갑을 생산합니다.
/// 광물 1개당 2초 후 수갑 1개 생산.
/// 플레이어가 진입하면 쌓인 수갑을 전부 픽업합니다.
/// </summary>
namespace PlayableAd.Systems
{
    public class HandcuffFactory : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField]
        private OreDepositZone depositZone;

        [Header("수갑 생산 설정")]
        [SerializeField] 
        private float craftTimePerOre = 2f;

        [Header("수갑 스택 설정")]
        [SerializeField]
        private Transform handcuffStackRoot;
        [SerializeField]
        private float columnSpacing = 0.4f;
        [SerializeField]
        private float rowSpacing = 0.4f;
        [SerializeField]
        private float stackYSpacing = 0.15f;

        private readonly List<GameObject> _producedHandcuffs = new();
        private bool _isProducing;
        private Coroutine _pickupCoroutine;

        public int ProducedCount => _producedHandcuffs.Count;

        private void OnEnable()
        {
            // OreDepositZone 반납 완료 이벤트 구독
            depositZone.OnOresDeposited += HandleOresDeposited;
        }

        private void OnDisable()
        {
            depositZone.OnOresDeposited -= HandleOresDeposited;
        }

        // -------------------------------------------------------
        // 생산 루틴
        // -------------------------------------------------------

        private void HandleOresDeposited(int oreCount)
        {
            if (_isProducing) return;
            StartCoroutine(ProduceRoutine(oreCount));
        }

        private IEnumerator ProduceRoutine(int oreCount)
        {
            _isProducing = true;

            for (int i = 0; i < oreCount; i++)
            {
                yield return new WaitForSeconds(craftTimePerOre);

                // 반납된 광물 오브젝트 하나 회수
                var ore = depositZone.TakeOneOre();
                if (ore != null)
                    ObjectPool.Instance.Return(ore);

                // 수갑 생산 + 논리 수량 추가
                ProduceHandcuff(_producedHandcuffs.Count);
                ResourceManager.Instance.TryAdd(ResourceType.Handcuff, 1);
                GameEventBus.Publish_HandcuffCrafted(1);

                if (_producedHandcuffs.Count == 1)
                    GameManager.Instance.AdvanceTo(GameStage.Stage3_Sales);

            }

            _isProducing = false;
        }
        private void ProduceHandcuff(int index)
        {
            if (handcuffStackRoot == null) return;

            var targetPos = CalculateHandcuffPosition(index);
            var go = ObjectPool.Instance.Get(
                                "Handcuff",
                                handcuffStackRoot.position,
                                Quaternion.identity);
            if (go == null) return;

            _producedHandcuffs.Add(go);

            if (go.TryGetComponent<ItemMover>(out var mover))
                mover.MoveTo(targetPos);
            else
                go.transform.position = targetPos;
        }

        private Vector3 CalculateHandcuffPosition(int index)
        {
          

            int col = index % 1;
            int row = index / 1;
            int height = row / 1;
            int depth = row % 1;

            float xOffset = (col == 0 ? -1f : 1f) * (columnSpacing * 0.5f);
            float zOffset = depth * rowSpacing;
            float yOffset = height * stackYSpacing;

            return handcuffStackRoot.position
                 + handcuffStackRoot.right * xOffset  // 두 줄 X 간격
                 + handcuffStackRoot.forward * zOffset  // 앞뒤 Z 간격
                 + Vector3.up * yOffset;  // Y축 높이
        }
        /// <summary>WorkerNPC가 도착 시 직접 호출 — 픽업 시작</summary>
        public void StartPickupForWorker(WorkerNPC worker)
        {
            if (_pickupCoroutine != null)
                StopCoroutine(_pickupCoroutine);

            _pickupCoroutine = StartCoroutine(PickupRoutine(
                canContinue: () => worker.CanPickupHandcuff,
                onPickup: go => worker.ReceiveHandcuff(go),
                maxPickup: 10
            ));
        }

        /// <summary>WorkerNPC가 떠날 때 픽업 중단</summary>
        public void StopPickupForWorker()
        {
            if (_pickupCoroutine != null)
            {
                StopCoroutine(_pickupCoroutine);
                _pickupCoroutine = null;
            }
        }

        // -------------------------------------------------------
        // 플레이어 진입 → 수갑 픽업
        // -------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            // 플레이어
            if (other.CompareTag("Player"))
            {
                var inventory = other.GetComponent<PlayerInventory>();
                if (inventory == null) return;

                _pickupCoroutine = StartCoroutine(PickupRoutine(
                    canContinue: () => !PrisonManager.Instance.IsFull,
                    onPickup: go => inventory.ReceiveHandcuff(go),
                    maxPickup: int.MaxValue
                ));
                return;
            }

            // WorkerNPC
            var worker = other.GetComponent<WorkerNPC>();
            if (worker == null) return;

            _pickupCoroutine = StartCoroutine(PickupRoutine(
                canContinue: () => worker.CanPickupHandcuff,
                onPickup: go => worker.ReceiveHandcuff(go),
                maxPickup: 10
            ));
        }
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponent<WorkerNPC>() == null)
                return;

            if (_pickupCoroutine != null)
            {
                StopCoroutine(_pickupCoroutine);
                _pickupCoroutine = null;
            }
        }

        private IEnumerator PickupRoutine(
            Func<bool> canContinue,
            Action<GameObject> onPickup,
            int maxPickup)
        {
            int pickedUp = 0;

            while (pickedUp < maxPickup)
            {
                if (!canContinue()) yield break;

                // 수갑 생산 대기
                while (_producedHandcuffs.Count == 0)
                {
                    if (!canContinue()) yield break;
                    yield return new WaitForSeconds(0.1f);
                }

                var go = _producedHandcuffs[^1];
                _producedHandcuffs.RemoveAt(_producedHandcuffs.Count - 1);

                if (go != null)
                {
                    onPickup(go);
                    pickedUp++;
                }

                yield return new WaitForSeconds(0.08f);
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (depositZone == null) return;

            Gizmos.color = Color.green;
            for (int i = 0; i < 10; i++)
            {
                var pos = CalculateHandcuffPosition(i);
                Gizmos.DrawWireCube(pos, Vector3.one * 0.2f);
            }
        }
#endif
    }

} 