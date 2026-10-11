// Assets/_Project/Scripts/Player/WeaponVisual.cs
using PlayableAd.Core;
using PlayableAd.Interface;
using UnityEngine;

namespace PlayableAd.Player
{
    /// <summary>
    /// OreZone 진입 시 무기 오브젝트를 활성화/비활성화합니다.
    /// 무기 업그레이드 시 해당 단계 무기로 교체됩니다.
    /// </summary>
    public class WeaponVisual : MonoBehaviour
    {
        [Header("무기 오브젝트 (단계별)")]
        [SerializeField] private GameObject weaponDefault;  // 기본 무기
        [SerializeField] private GameObject weaponLevel1;   // Weapon1 업그레이드 후
        [SerializeField] private GameObject weaponLevel2;   // Weapon2 업그레이드 후

        private GameObject _currentWeapon;
        private int _weaponLevel = 0;

        private void Awake()
        {
            // 시작 시 모두 비활성화
            weaponDefault?.SetActive(false);
            weaponLevel1?.SetActive(false);
            weaponLevel2?.SetActive(false);

            _currentWeapon = weaponDefault;
        }

        private void OnEnable()
        {
            GameEventBus.OnUpgradePurchased += HandleUpgrade;
        }

        private void OnDisable()
        {
            GameEventBus.OnUpgradePurchased -= HandleUpgrade;
        }

        // -------------------------------------------------------
        // OreZone에서 호출
        // -------------------------------------------------------

        public void ShowWeapon()
        {
            _currentWeapon?.SetActive(true);
        }

        public void HideWeapon()
        {
            _currentWeapon?.SetActive(false);
        }

        // -------------------------------------------------------
        // 업그레이드 시 무기 교체
        // -------------------------------------------------------

        private void HandleUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Weapon1:
                    SwitchWeapon(weaponLevel1, 1);
                    break;
                case UpgradeType.Weapon2:
                    SwitchWeapon(weaponLevel2, 2);
                    break;
            }
        }

        private void SwitchWeapon(GameObject newWeapon, int level)
        {
            bool wasShowing = _currentWeapon != null && _currentWeapon.activeSelf;

            // 현재 무기 숨기기
            _currentWeapon?.SetActive(false);

            // 새 무기로 교체
            _currentWeapon = newWeapon;
            _weaponLevel = level;

            // 이전에 보이고 있었으면 새 무기도 바로 표시
            if (wasShowing)
                _currentWeapon?.SetActive(true);
        }
    }
}