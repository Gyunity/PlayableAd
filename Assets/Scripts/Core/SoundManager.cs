using UnityEngine;
using PlayableAd.Utils;
using PlayableAd.Interface;

namespace PlayableAd.Core
{
    public class SoundManager : Singleton<SoundManager>
    {
        // -------------------------------------------------------
        // 사운드 클립 + 볼륨 묶음 클래스
        // -------------------------------------------------------
        [System.Serializable]
        public class SoundEntry
        {
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [Header("채굴 사운드 (무기별)")]
        [SerializeField] private SoundEntry mineDefault;  // 기본 채굴
        [SerializeField] private SoundEntry mineWeapon1;  // 무기1
        [SerializeField] private SoundEntry mineWeapon2;  // 무기2

        [Header("아이템 사운드")]
        [SerializeField] private SoundEntry oreDepositSound;      // 광석 내려놓기
        [SerializeField] private SoundEntry handcuffDepositSound; // 수갑 내려놓기

        [Header("돈 사운드")]
        [SerializeField] private SoundEntry coinEarnSound;     // 돈 얻기
        [SerializeField] private SoundEntry coinPaySound;      // 돈 내기
        [SerializeField] private SoundEntry purchaseComplete;  // 구매 완료

        [Header("마스터 볼륨")]
        [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;

        [Header("채굴 소리 쿨타임")]
        [SerializeField] private float mineCooldown = 0.1f; // 이 시간 안에 중복 재생 방지
        private float _lastMineTime = -999f;
        private AudioSource _audioSource;
        private int _weaponLevel = 0;
        private int _lastBalance = 0;

        protected override void OnAwake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            ServiceLocator.Register<SoundManager>(this);
        }

        private void OnEnable()
        {
            GameEventBus.OnOreHarvested += HandleOreHarvested;
            GameEventBus.OnCurrencyChanged += HandleCurrencyChanged;
            GameEventBus.OnUpgradePurchased += HandleUpgrade;
            GameEventBus.OnItemSold += HandleItemSold;
        }

        private void OnDisable()
        {
            GameEventBus.OnOreHarvested -= HandleOreHarvested;
            GameEventBus.OnCurrencyChanged -= HandleCurrencyChanged;
            GameEventBus.OnUpgradePurchased -= HandleUpgrade;
            GameEventBus.OnItemSold -= HandleItemSold;
        }

        // -------------------------------------------------------
        // 이벤트 핸들러
        // -------------------------------------------------------

        private void HandleOreHarvested(int amount)
        {
            if (Time.time - _lastMineTime < mineCooldown) return;

            _lastMineTime = Time.time;
            switch (_weaponLevel)
            {
                case 0: Play(mineDefault); break;
                case 1: Play(mineWeapon1); break;
                case 2: Play(mineWeapon2); break;
            }
        }

        private void HandleHandcuffDeposit(int amount)
        {
            Play(handcuffDepositSound);
        }

        private void HandleCurrencyChanged(int balance)
        {
            if (balance > _lastBalance)
                Play(coinEarnSound);
            else
                Play(coinPaySound);

            _lastBalance = balance;
        }

        private void HandleUpgrade(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Weapon1: _weaponLevel = 1; break;
                case UpgradeType.Weapon2: _weaponLevel = 2; break;
            }
            Play(purchaseComplete);
        }

        private void HandleItemSold(int amount)
        {
            Play(coinPaySound);
        }

        // -------------------------------------------------------
        // 외부 직접 호출용
        // -------------------------------------------------------

        public void PlayOreDeposit() => Play(oreDepositSound);
        public void PlayHandcuffDeposit() => Play(handcuffDepositSound);
        public void PlayCoinEarn() => Play(coinEarnSound);
        public void PlayCoinPay() => Play(coinPaySound);
        public void PlayPurchaseDone() => Play(purchaseComplete);

        // -------------------------------------------------------
        // 재생 — 개별 볼륨 * 마스터 볼륨
        // -------------------------------------------------------

        private void Play(SoundEntry entry)
        {
            if (entry == null || entry.clip == null) return;
            _audioSource.PlayOneShot(entry.clip, entry.volume * masterVolume);
        }
    }
}