using PlayableAd.Core;
using PlayableAd.Interface;
using UnityEngine;

namespace PlayableAd.Systems
{
    /// <summary>
    /// NPC 광부(MinerNPC)에 붙는 채굴 컴포넌트.
    /// PlayerMiner와 동일하게 Physics.OverlapSphere로 주변 광물을 직접 탐색합니다.
    /// </summary>
    public class MiningSystem : MonoBehaviour, IUpgradeable
    {
        [Header("채굴 설정")]
        [SerializeField] private float miningInterval = 0.5f;
        [SerializeField] private float miningRange = 1.5f;
        [SerializeField] private LayerMask oreLayer;  // 인스펙터에서 "Ore" 레이어 선택

        private float _timer;
        private bool _isActive;

        public UpgradeType UpgradeType => UpgradeType.Weapon1;
        public bool IsUpgraded { get; private set; }

        private void OnEnable()
        {
            GameEventBus.OnUpgradePurchased += HandleUpgrade;
        }

        private void OnDisable()
        {
            GameEventBus.OnUpgradePurchased -= HandleUpgrade;
        }

        public void Activate() => _isActive = true;
        public void Deactivate() => _isActive = false;

        private void Update()
        {
            if (!_isActive) return;

            _timer += Time.deltaTime;
            if (_timer >= miningInterval)
            {
                _timer = 0f;
                TryMine();
            }
        }

        private void TryMine()
        {
            // ResourceManager Max 체크
            var current = ResourceManager.Instance.GetAmount(ResourceType.Ore);
            var max = ResourceManager.Instance.GetMax(ResourceType.Ore);
            if (current >= max) return;

            // 앞 방향 기준으로 OverlapSphere 탐색
            var checkPos = transform.position + transform.forward * (miningRange * 0.5f);
            var hits = Physics.OverlapSphere(checkPos, miningRange, oreLayer);

            if (hits.Length == 0) return;

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent<ICollectable>(out var ore)) continue;
                if (!ore.IsCollectable) continue;

                if (ore.Collect(gameObject))
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            var checkPos = transform.position + transform.forward * (miningRange * 0.5f);
            Gizmos.DrawWireSphere(checkPos, miningRange);
        }

        public void ApplyUpgrade()
        {
            miningInterval = Mathf.Max(0.1f, miningInterval - 0.2f);
            miningRange += 1.0f;
            IsUpgraded = true;
        }

        private void HandleUpgrade(UpgradeType type)
        {
            if (type == UpgradeType.Weapon1 || type == UpgradeType.Weapon2)
                ApplyUpgrade();
        }
    }
}