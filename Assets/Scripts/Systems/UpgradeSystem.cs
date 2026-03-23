using PlayableAd.Core;
using PlayableAd.Data;
using PlayableAd.Interface;
using PlayableAd.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Systems
{

    public class UpgradeSystem : MonoBehaviour, IPurchasable
    {
        [Header("업그레이드 설정")]
        [SerializeField]
        private UpgradeType upgradeType;
        [SerializeField]
        private int price;
        [SerializeField]
        private MiningConfig config;

        public int Price => price;
        public bool IsPurchased {  get; private set; }
        public bool CanPurchase(int currentCurrency)
            => !IsPurchased && currentCurrency >= price;

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

            var miner = other.GetComponent<PlayerMiner>();

            if (miner == null) return;

            switch (upgradeType)
            {
                case UpgradeType.Weapon1:
                    miner.ApplyWeapon1Upgrade(config);
                    break;
                case UpgradeType.Weapon2:
                    miner.ApplyWeapon2Upgrade(config);
                    break;
            }

            Purchase();
            gameObject.SetActive(false);

            Debug.Log($"[UpgradeSystem] {upgradeType} 완료!");
        }



    }

}