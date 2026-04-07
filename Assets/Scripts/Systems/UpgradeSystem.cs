using PlayableAd.Core;
using PlayableAd.Data;
using PlayableAd.Interface;
using PlayableAd.Player;
using PlayableAd.UI;
using System.Collections;
using UnityEngine;

namespace PlayableAd.Systems
{
    public class UpgradeSystem : MonoBehaviour, IPurchasable
    {
        [Header("업그레이드 설정")]
        [SerializeField] private UpgradeType upgradeType;
        [SerializeField] private int price;
        [SerializeField] private MiningConfig config;

        [Header("가격 표시")]
        [SerializeField] private ShopPriceDisplay priceDisplay;

        public int Price => price;
        public bool IsPurchased { get; private set; }

        private bool _playerInZone = false;
        private Transform _playerTransform;
        private Collider _playerCollider;
        private Coroutine _payCoroutine;

        public bool CanPurchase(int currentCurrency)
            => !IsPurchased && currentCurrency >= 0;

        public void Purchase()
        {
            IsPurchased = true;
            GameEventBus.Publish_UpgradePurchased(upgradeType);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (IsPurchased) return;

            _playerInZone = true;
            _playerTransform = other.transform;
            _playerCollider = other;

            _payCoroutine = StartCoroutine(PayWhileInZone());
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerInZone = false;
            _playerTransform = null;
            _playerCollider = null;

            if (_payCoroutine != null)
            {
                StopCoroutine(_payCoroutine);
                _payCoroutine = null;
            }
        }

        private IEnumerator PayWhileInZone()
        {
            while (_playerInZone && !IsPurchased)
            {
                if (priceDisplay == null)
                {
                    // priceDisplay 없으면 기존 방식으로 즉시 구매
                    if (CurrencyManager.Instance.TrySpend(price))
                        ExecuteUpgrade();
                    yield break;
                }

                if (priceDisplay.IsPaid)
                {
                    ExecuteUpgrade();
                    yield break;
                }

                if (CurrencyManager.Instance.Balance > 0)
                    priceDisplay.TryPay(_playerTransform);

                yield return new WaitForSeconds(0.05f);
            }
        }

        private void ExecuteUpgrade()
        {
            if (IsPurchased) return;
            if (_playerCollider == null) return;

            var miner = _playerCollider.GetComponent<PlayerMiner>();
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