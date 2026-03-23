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

        [Header("광물 탐색 레이어")]
        [SerializeField] private LayerMask oreLayer;

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

        private void Update()
        {
            _miningTimer += Time.deltaTime;
            if (_miningTimer >= _currentInterval)
            {
                _miningTimer = 0f;
                TryMine();
            }
        }

        private void TryMine()
        {
            // Max 체크
            var current = ResourceManager.Instance.GetAmount(ResourceType.Ore);
            var max = ResourceManager.Instance.GetMax(ResourceType.Ore);
            if (current >= max) return;

            // Physics로 플레이어 앞 범위에서 Ore 레이어 오브젝트 탐색
            var checkPos = transform.position + transform.forward * (_currentRange * 0.5f);
            var hits = Physics.OverlapSphere(checkPos, _currentRange, oreLayer);

            foreach (var hit in hits)
            {
                // Max 재체크 — 수집 중 Max 도달 시 중단
                current = ResourceManager.Instance.GetAmount(ResourceType.Ore);
                if (current >= max) break;

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
            Debug.Log($"[PlayerMiner] Weapon1 → 간격:{_currentInterval}s / 범위:{_currentRange} / Max:{cfg.weapon1MaxOre}");
        }

        public void ApplyWeapon2Upgrade(MiningConfig cfg)
        {
            _currentInterval = cfg.weapon2Interval;
            _currentRange = cfg.weapon2Range;
            ResourceManager.Instance.SetMax(ResourceType.Ore, cfg.weapon2MaxOre);

            IsUpgraded = true;
            Debug.Log($"[PlayerMiner] Weapon2 → 간격:{_currentInterval}s / 범위:{_currentRange} / Max:{cfg.weapon2MaxOre}");
        }
        public void ApplyUpgrade()
        {
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var checkPos = transform.position + transform.forward * (_currentRange * 0.5f);
            Gizmos.DrawWireSphere(checkPos, _currentRange);
        }

       
#endif
    }
}