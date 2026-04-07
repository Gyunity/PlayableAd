using PlayableAd.Core;
using PlayableAd.Data;
using PlayableAd.Interface;
using UnityEngine;

namespace PlayableAd.Player
{
    [RequireComponent(typeof(PlayerInventory))]
    public class PlayerMiner : MonoBehaviour, IUpgradeable
    {
        [SerializeField] private MiningConfig config;
        [SerializeField] private LayerMask oreLayer;

        private bool _inOreZone = false;

        private PlayerInventory _inventory;
        private float _miningTimer;
        private float _currentInterval;
        private float _currentRange;

        public UpgradeType UpgradeType => UpgradeType.Weapon1;
        public bool IsUpgraded { get; private set; }

        private void Awake()
        {
            _inventory = GetComponent<PlayerInventory>();
            _currentInterval = config.miningInterval;
            _currentRange = config.miningRange;
        }

        private void OnEnable()
        {
            GameEventBus.OnUpgradePurchased += HandleUpgrade;
        }

        private void OnDisable()
        {
            GameEventBus.OnUpgradePurchased -= HandleUpgrade;
        }

        private void Update()
        {
            if (!_inOreZone) return;

            _miningTimer += Time.deltaTime;
            if (_miningTimer >= _currentInterval)
            {
                _miningTimer = 0f;
                TryMine();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("OreZone"))
                _inOreZone = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("OreZone"))
            {
                _inOreZone = false;
                _miningTimer = 0f;
            }
        }

        private void TryMine()
        {
            var checkPos = transform.position
                         + transform.forward * (_currentRange * 0.5f);
            var hits = Physics.OverlapSphere(checkPos, _currentRange, oreLayer);

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent<ICollectable>(out var ore)) continue;
                if (!ore.IsCollectable) continue;

                ore.Collect(gameObject);

                if (!IsUpgraded) break;
            }
        }

        public void ApplyWeapon1Upgrade(MiningConfig cfg)
        {
            _currentInterval = cfg.weapon1Interval;
            _currentRange = cfg.weapon1Range;
            ResourceManager.Instance.SetMax(ResourceType.Ore, cfg.weapon1MaxOre);
            IsUpgraded = true;
        }

        public void ApplyWeapon2Upgrade(MiningConfig cfg)
        {
            _currentInterval = cfg.weapon2Interval;
            _currentRange = cfg.weapon2Range;
            ResourceManager.Instance.SetMax(ResourceType.Ore, cfg.weapon2MaxOre);
            IsUpgraded = true;
        }

        public void ApplyRangeUpgrade(float multiplier)
        {
            _currentRange *= multiplier;
        }

        public void ApplyUpgrade() { }

        private void HandleUpgrade(UpgradeType type)
        {
            // UpgradeSystem에서 직접 호출하므로 여기선 불필요
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var checkPos = transform.position
                         + transform.forward * (_currentRange * 0.5f);
            Gizmos.DrawWireSphere(checkPos, _currentRange);
        }
#endif
    }
}