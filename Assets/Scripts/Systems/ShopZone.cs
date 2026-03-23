using UnityEngine;
using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.NPC;
using System.Xml.Serialization;
using System.Collections;

namespace PlayableAd.Systems
{
    /// <summary>
    /// NPC 구매 존 공통 클래스.
    /// UpgradeType에 따라 광부/동료/수용소 확장을 처리합니다.
    /// </summary>
    public class ShopZone : MonoBehaviour, IPurchasable
    {
        [Header("구매 설정")]
        [SerializeField]
        private UpgradeType upgradeType;
        [SerializeField]
        private int price;

        [Header("NPC 스폰")]
        [SerializeField]
        private GameObject minerNPCPrefab;
        [SerializeField]
        private GameObject workerNPCPrefab;
        [SerializeField]
        private Transform[] spawnPoints;

        [Header("NPC에게 주입할 씬 참조")]
        [SerializeField] 
        private OreDepositZone depositZone;  
        [SerializeField] 
        private HandcuffFactory factory;    
        [SerializeField] 
        private SalesZone salesZone;   
        public int Price => price;
        public bool IsPurchased { get; private set; }

        public bool CanPurchase(int currency)
            => !IsPurchased && currency >= price;

        public void Purchase()
        {
            IsPurchased = true;
            GameEventBus.Publish_UpgradePurchased(upgradeType);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (!CanPurchase(CurrencyManager.Instance.Balance)) return;
            if (!CurrencyManager.Instance.TrySpend(price)) return;

            switch (upgradeType)
            {
                case UpgradeType.Miner:
                    SpawnMiners();
                    Purchase();
                    gameObject.SetActive(false);
                    break;

                case UpgradeType.Worker:
                    SpawnWorker();
                    Purchase();
                    gameObject.SetActive(false);
                    break;

                case UpgradeType.PrisonExpand:
                    ExpandPrison();
                    // PrisonExpand는 비활성화 안 함 — GameManager가 Stage9로 전환
                    break;
            }
        }

        private void SpawnMiners()
        {
            int count = Mathf.Min(3, spawnPoints.Length);
            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(
                    minerNPCPrefab,
                    spawnPoints[i].position,
                    spawnPoints[i].rotation);
                var npc = go.GetComponent<MinerNPC>();
                if (npc == null)
                    continue;
                npc.Init(depositZone);
                npc.StartWorkDelayed();
            }
            Purchase();
            gameObject.SetActive(false);
            GameEventBus.Publish_NPCSpawned(NPCRole.Miner, count);
            Debug.Log($"[ShopZone] 광부 {count}명 스폰");
        }

        private void SpawnWorker()
        {
            if (spawnPoints.Length == 0) return;

            var go = Instantiate(
                          workerNPCPrefab,
                          spawnPoints[0].position,
                          spawnPoints[0].rotation);
            var npc = go.GetComponent<WorkerNPC>();

            if (npc == null)
                return;
            npc.Init(factory, salesZone);
            npc.StartWorkDelayed(); 

            Purchase();
            gameObject.SetActive(false);
            GameEventBus.Publish_NPCSpawned(NPCRole.FactoryWorker, 1);
            Debug.Log("[ShopZone] 동료 1명 스폰");
        }

        private void ExpandPrison()
        {
            PrisonManager.Instance.ExpandCapacity(20);
            Debug.Log("[ShopZone] 수용소 확장!");

            // CameraDirector가 OnPrisonExpanded 이벤트로 연출 처리
            // GameManager가 Stage9로 전환 → EndScreenUI 표시
            GameManager.Instance.AdvanceTo(GameStage.Stage9_End);
        }

     

    }

}