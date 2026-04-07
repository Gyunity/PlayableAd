using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.NPC;
using PlayableAd.Player;
using PlayableAd.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


namespace PlayableAd.Systems
{

    public class SalesZone : MonoBehaviour, ISellable
    {
        [Header("수갑 배치 설정")]
        [SerializeField] private Transform depositRoot;
        [SerializeField] private float columnSpacing = 0.4f;
        [SerializeField] private float rowSpacing = 0.4f;
        [SerializeField] private float stackYSpacing = 0.15f;

        [SerializeField] private Image border;

        // --- ISellable ---
        public ResourceType SellResourceType => ResourceType.Handcuff;
        public int SellAmountPerCustomer => 4;
        public int RevenuePerCustomer => 100;

        private readonly List<GameObject> _depositedHandcuffs = new();

        // -------------------------------------------------------
        // 수갑 내려놓기 (플레이어/WorkerNPC가 존 진입 시)
        // -------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            // 플레이어
            if (other.CompareTag("Player"))
            {
                border.color = new Color(0.2f, 0.8f, 0.2f, 0.6f);

                var inventory = other.GetComponent<PlayerInventory>();
                if (inventory == null) return;

                if (inventory.HandcuffVisualCount <= 0) return;

                var handcuffs = inventory.TakeAllHandcuffs();
                StartCoroutine(DepositHandcuffs(handcuffs));
                return;
            }

            // WorkerNPC
            var worker = other.GetComponent<WorkerNPC>();
            if (worker == null) return;

            border.color = new Color(0.2f, 0.8f, 0.2f, 0.6f);

            if (worker.HandcuffCount <= 0) return;

            var workerHandcuffs = worker.TakeAllHandcuffs();
            StartCoroutine(DepositHandcuffs(workerHandcuffs));
        }
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponent<WorkerNPC>() == null)
                return;
            border.color = Color.white;

        }
        public IEnumerator DepositHandcuffs(List<GameObject> handcuffs)
        {
            for (int i = 0; i < handcuffs.Count; i++)
            {
                var go = handcuffs[i];
                if (go == null) continue;

                var targetPos = CalculateDepositPosition(_depositedHandcuffs.Count);
                go.transform.SetParent(null);

                if (go.TryGetComponent<ItemMover>(out var mover))
                    mover.MoveTo(targetPos, onComplete: () =>
                    {
                        go.transform.rotation = depositRoot.rotation;
                        CoroutineRunner.Instance.Run(
                  ScalePopUtility.ScalePop(go));
                    });
                else
                {
                    go.transform.position = targetPos;
                    go.transform.rotation = depositRoot.rotation;

                    CoroutineRunner.Instance.Run(
                  ScalePopUtility.ScalePop(go));
                }

                SoundManager.Instance.PlayHandcuffDeposit();
                _depositedHandcuffs.Add(go);

                yield return new WaitForSeconds(0.05f);
            }
        }

        // -------------------------------------------------------
        // CriminalNPC가 1개씩 구매 시 호출
        // -------------------------------------------------------

        /// <summary>수갑 1개 시각 오브젝트 제거</summary>
        public void RemoveOneDepositedHandcuff()
        {
            if (_depositedHandcuffs.Count == 0) return;

            var go = _depositedHandcuffs[^1];
            _depositedHandcuffs.RemoveAt(_depositedHandcuffs.Count - 1);

            if (go != null)
                ObjectPool.Instance.Return(go);
        }

        public void ClearDepositedHandcuffs()
        {
            foreach (var go in _depositedHandcuffs)
                if (go != null) ObjectPool.Instance.Return(go);

            _depositedHandcuffs.Clear();
        }

        public int DepositedCount => _depositedHandcuffs.Count;

        // -------------------------------------------------------
        // ISellable
        // -------------------------------------------------------

        public bool CanSell(IResourceContainer container)
        {
            return !PrisonManager.Instance.IsFull
                && container.GetAmount(SellResourceType) >= SellAmountPerCustomer;
        }

        // -------------------------------------------------------
        // 위치 계산
        // -------------------------------------------------------

        private Vector3 CalculateDepositPosition(int index)
        {
            int col = index % 1;
            int row = index / 1;
            int height = row / 1;
            int depth = row % 1;

            float xOffset = (col == 0 ? -1f : 1f) * (columnSpacing * 0.5f);
            float zOffset = depth * rowSpacing;
            float yOffset = height * stackYSpacing;

            return depositRoot.position
                 + depositRoot.right * xOffset  // 두 줄 X 간격
                 + depositRoot.forward * zOffset  // 앞뒤 Z 간격
                 + Vector3.up * yOffset;  // Y축 높이
        }
    }

}

