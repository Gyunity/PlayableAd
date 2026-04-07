using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.NPC;
using PlayableAd.UI;
using System.Collections;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

namespace PlayableAd.Systems
{
    /// <summary>
    /// NPC 구매 존 공통 클래스.
    /// UpgradeType에 따라 광부/동료/수용소 확장을 처리합니다.
    /// </summary>
    public class ShopZone : MonoBehaviour, IPurchasable
    {
        [Header("구매 설정")]
        [SerializeField] private UpgradeType upgradeType;
        [SerializeField] private int price;

        [Header("NPC 스폰")]
        [SerializeField] private GameObject minerNPCPrefab;
        [SerializeField] private GameObject workerNPCPrefab;
        [SerializeField] private Transform[] spawnPoints;

        [Header("NPC 주입 참조")]
        [SerializeField] private OreDepositZone depositZone;
        [SerializeField] private HandcuffFactory factory;
        [SerializeField] private SalesZone salesZone;
        //[SerializeField] private CoinZone coinZone;

        [Header("가격 표시")]
        [SerializeField] private ShopPriceDisplay priceDisplay;

        public int Price => price;
        public bool IsPurchased { get; private set; }

        private bool _playerInZone = false;
        private Transform _playerTransform;
        private Coroutine _payCoroutine;

        public bool CanPurchase(int currency)
            => !IsPurchased && currency >= 0;

        public void Purchase()
        {
            IsPurchased = true;
            GameEventBus.Publish_UpgradePurchased(upgradeType);
        }

        // -------------------------------------------------------
        // 트리거
        // -------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (IsPurchased) return;

            _playerInZone = true;
            _playerTransform = other.transform;

            _payCoroutine = StartCoroutine(PayWhileInZone());
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInZone = false;
            _playerTransform = null;

            if (_payCoroutine != null)
            {
                StopCoroutine(_payCoroutine);
                _payCoroutine = null;
            }
        }

        // -------------------------------------------------------
        // 존 안에 있는 동안 계속 지불
        // -------------------------------------------------------

        private IEnumerator PayWhileInZone()
        {
            while (_playerInZone && !IsPurchased)
            {
                if (priceDisplay == null)
                {
                    // priceDisplay 없으면 기존 방식으로 즉시 구매
                    if (CurrencyManager.Instance.TrySpend(price))
                        ExecutePurchase();
                    yield break;
                }

                if (priceDisplay.IsPaid)
                {
                    ExecutePurchase();
                    yield break;
                }

                if (CurrencyManager.Instance.Balance > 0)
                    priceDisplay.TryPay(_playerTransform);

                yield return new WaitForSeconds(0.05f);
            }
        }

        // -------------------------------------------------------
        // 구매 실행
        // -------------------------------------------------------

        private void ExecutePurchase()
        {
            if (IsPurchased) return;

            switch (upgradeType)
            {
                case UpgradeType.Miner:
                    SpawnMiners();
                    break;
                case UpgradeType.Worker:
                    SpawnWorker();
                    break;
                case UpgradeType.PrisonExpand:
                    ExpandPrison();
                    return; // 비활성화 안 함
            }

            Purchase();
            gameObject.SetActive(false);
        }

        private void SpawnMiners()
        {
            int count = Mathf.Min(3, spawnPoints.Length);
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(minerNPCPrefab,
                              spawnPoints[i].position,
                              spawnPoints[i].rotation);
                var npc = go.GetComponent<MinerNPC>();
                if (npc == null) continue;

                npc.Init(depositZone);
                npc.StartWorkDelayed();
            }

            GameEventBus.Publish_NPCSpawned(NPCRole.Miner, count);
        }

        private void SpawnWorker()
        {
            if (spawnPoints.Length == 0) return;

            var go = Instantiate(workerNPCPrefab,
                          spawnPoints[0].position,
                          spawnPoints[0].rotation);
            var npc = go.GetComponent<WorkerNPC>();
            if (npc == null) return;

            npc.Init(factory, salesZone);
            npc.StartWorkDelayed();

            GameEventBus.Publish_NPCSpawned(NPCRole.FactoryWorker, 1);
        }

        private void ExpandPrison()
        {
            PrisonManager.Instance.ExpandCapacity(20);
            GameEventBus.Publish_PrisonExpanded();
        }

    }

}