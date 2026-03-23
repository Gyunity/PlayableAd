using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Items;
using PlayableAd.NPC;
using PlayableAd.Player;
using PlayableAd.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlayableAd.Systems
{

    public class SalesZone : MonoBehaviour, ISellable
    {
        [Header("판매 설정")]
        [SerializeField]
        private int sellAmountPerCustomer = 4;    
        [SerializeField]
        private int revenuePerCustomer = 100;  
        [SerializeField]
        private float saleInterval = 0.8f;

        [Header("수갑 내려놓는 위치")]
        [SerializeField] private Transform depositRoot;
        [SerializeField] private float columnSpacing = 0.4f;
        [SerializeField] private float rowSpacing = 0.4f;

        // --- ISellable ---
        public ResourceType SellResourceType => ResourceType.Handcuff;
        public int SellAmountPerCustomer => sellAmountPerCustomer;
        public int RevenuePerCustomer => revenuePerCustomer;
        public bool IsSelling => _isSelling;

        // 누적 고객 수 (게임 전체에서 계속 카운팅)
        public int TotalCustomerCount { get; private set; } = 0;

        private readonly List<GameObject> _depositedHandcuffs = new();
        private bool _isSelling;
        private Coroutine _saleCoroutine;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[SalesZone] 트리거 감지: {other.gameObject.name}");

            // 플레이어
            if (other.CompareTag("Player"))
            {
                var inventory = other.GetComponent<PlayerInventory>();
                if (inventory == null) return;

                StartCoroutine(DepositAndSellRoutine(
                    getHandcuffCount: () => inventory.HandcuffVisualCount,
                    takeHandcuffs: count => TakeHandcuffsFromPlayer(inventory, count)
                ));
                return;
            }

            // WorkerNPC
            var worker = other.GetComponent<WorkerNPC>();
            if (worker == null)
            {
                Debug.Log($"[SalesZone] WorkerNPC 없음: {other.gameObject.name}");

                return;
            }
            Debug.Log($"[SalesZone] WorkerNPC 진입 / 수갑 수: {worker.HandcuffCount}");

            StartCoroutine(DepositAndSellRoutine(
                getHandcuffCount: () => worker.HandcuffCount,
                takeHandcuffs: count => TakeHandcuffsFromWorker(worker, count)
            ));
        }

        private IEnumerator DepositAndSellRoutine(
           System.Func<int> getHandcuffCount,
           System.Func<int, List<GameObject>> takeHandcuffs)
        {
            if (PrisonManager.Instance.IsFull) yield break;
            if (_isSelling) yield break;

            int count = getHandcuffCount();
            if (count <= 0) yield break;

            var handcuffs = takeHandcuffs(count);
            yield return StartCoroutine(DepositHandcuffs(handcuffs));

            _isSelling = true;
            _saleCoroutine = StartCoroutine(SellRoutine());
        }
        // 수갑 오브젝트를 SalesZone 앞에 배치
        private IEnumerator DepositHandcuffs(List<GameObject> handcuffs)
        {
            for (int i = 0; i < handcuffs.Count; i++)
            {
                var go = handcuffs[i];
                if (go == null) continue;

                var targetPos = CalculateDepositPosition(_depositedHandcuffs.Count);

                go.transform.SetParent(null);

                if (go.TryGetComponent<ItemMover>(out var mover))
                    mover.MoveTo(targetPos);
                else
                    go.transform.position = targetPos;

                _depositedHandcuffs.Add(go);

                yield return new WaitForSeconds(0.05f); // 살짝 딜레이로 자연스럽게
            }

            yield return new WaitForSeconds(0.3f); // 내려놓기 완료 후 잠시 대기
        }

        // 4개씩 자동 판매
        private IEnumerator SellRoutine()
        {
            var container = ServiceLocator.Get<IResourceContainer>();

            while (_depositedHandcuffs.Count >= sellAmountPerCustomer)
            {
                if (PrisonManager.Instance.IsFull) break;

                // 수갑 4개 제거
                RemoveDepositedHandcuffs(sellAmountPerCustomer);

                // 논리 수량 제거 + 수익 발행
                container.TryRemove(SellResourceType, sellAmountPerCustomer);
                GameEventBus.Publish_ItemSold(sellAmountPerCustomer);

                TotalCustomerCount++;
                GameEventBus.Publish_CustomerServed(TotalCustomerCount);

                yield return new WaitForSeconds(saleInterval);
            }

            // 남은 수갑 정리 (4개 미만)
            ClearDepositedHandcuffs();
            _isSelling = false;
        }
        private void RemoveDepositedHandcuffs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_depositedHandcuffs.Count == 0) break;

                var go = _depositedHandcuffs[^1];
                _depositedHandcuffs.RemoveAt(_depositedHandcuffs.Count - 1);

                if (go != null)
                    ObjectPool.Instance.Return(go);
            }
        }

        private void ClearDepositedHandcuffs()
        {
            foreach (var go in _depositedHandcuffs)
                if (go != null) ObjectPool.Instance.Return(go);

            _depositedHandcuffs.Clear();
        }

        private Vector3 CalculateDepositPosition(int index)
        {
            int col = index % 2;
            int row = index / 2;
            float xOffset = (col == 0 ? -1f : 1f) * (columnSpacing * 0.5f);
            float zOffset = row * rowSpacing;

            return depositRoot.position
                 + depositRoot.right * xOffset
                 + depositRoot.forward * zOffset;
        }

        // -------------------------------------------------------
        // 플레이어 / WorkerNPC에서 수갑 가져오기
        // -------------------------------------------------------

        private List<GameObject> TakeHandcuffsFromPlayer(
            PlayerInventory inventory, int count)
        {
            var list = inventory.TakeAllHandcuffs();

            // ResourceManager 논리 수량은 유지 (SellRoutine에서 차감)
            return list;
        }

        private List<GameObject> TakeHandcuffsFromWorker(
            WorkerNPC worker, int count)
        {
            return worker.TakeAllHandcuffs();
        }

        // -------------------------------------------------------
        // ISellable (외부 호출용 — 하위 호환)
        // -------------------------------------------------------

        public bool CanSell(IResourceContainer container)
        {
            return !PrisonManager.Instance.IsFull
                && container.GetAmount(SellResourceType) >= sellAmountPerCustomer;
        }

 
        public void StartSellForWorker(WorkerNPC worker)
        {
            if (_isSelling) return;

            StartCoroutine(DepositAndSellRoutine(
                getHandcuffCount: () => worker.HandcuffCount,
                takeHandcuffs: count => TakeHandcuffsFromWorker(worker, count)
            ));
        }

    }

}