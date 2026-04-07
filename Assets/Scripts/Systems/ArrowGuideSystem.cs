using PlayableAd.Core;
using PlayableAd.Interface;
using UnityEngine;

namespace PlayableAd.Systems
{
    public class ArrowGuideSystem : MonoBehaviour
    {
        [Header("존 위 화살표")]
        [SerializeField] private GameObject oreZoneIndicator;
        [SerializeField] private GameObject depositZoneIndicator;
        [SerializeField] private GameObject factoryIndicator;
        [SerializeField] private GameObject salesZoneIndicator;
        [SerializeField] private GameObject coinZoneIndicator;

        [Header("방향 화살표 (플레이어 위)")]
        [SerializeField] private Transform directionArrow;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float arrowHeight = 1.5f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.1f;

        [Header("목적지 Transform")]
        [SerializeField] private Transform oreZoneTarget;
        [SerializeField] private Transform depositZoneTarget;
        [SerializeField] private Transform factoryTarget;
        [SerializeField] private Transform salesZoneTarget;
        [SerializeField] private Transform coinZoneTarget;

        [Header("광석 임계값")]
        [SerializeField] private int oreThreshold = 4; // 광석 몇 개부터 가이드

        private enum GuideStep
        {
            OreZone,         // 시작 — 광석존 위 화살표
            DepositZone,     // 광석 4개 이상 — 반납존 + 방향
            HandcuffFactory, // 반납 완료 — 공장 위 화살표 + 방향
            SalesZone,       // 수갑 수령 — 판매존 위 화살표
            CoinZone,        // 수갑 판매 — 코인존 위 화살표
            Done             // 돈 수령 — 완료
        }

        private GuideStep _currentStep = GuideStep.OreZone;
        private Transform _currentTarget;
        private float _bobTimer;

        private void Start()
        {
            HideAllIndicators();
            directionArrow?.gameObject.SetActive(false);
            SetStep(GuideStep.OreZone);
        }

        private void OnEnable()
        {
            GameEventBus.OnOreHarvested += HandleOreHarvested;
            GameEventBus.OnOreDeposited += HandleOreDeposited;
            GameEventBus.OnHandcuffReceived += HandleHandcuffReceived; 
            GameEventBus.OnItemSold += HandleItemSold;
            GameEventBus.OnCurrencyChanged += HandleCurrencyChanged;
        }

        private void OnDisable()
        {
            GameEventBus.OnOreHarvested -= HandleOreHarvested;
            GameEventBus.OnOreDeposited -= HandleOreDeposited;
            GameEventBus.OnHandcuffReceived -= HandleHandcuffReceived; 
            GameEventBus.OnItemSold -= HandleItemSold;
            GameEventBus.OnCurrencyChanged -= HandleCurrencyChanged;
        }

        private void Update()
        {
            UpdateDirectionArrow();
        }

        // -------------------------------------------------------
        // 이벤트 핸들러
        // -------------------------------------------------------

        private void HandleOreHarvested(int amount)
        {
            if (_currentStep == GuideStep.OreZone)
            {
                oreZoneIndicator?.SetActive(false);
            }

            if (_currentStep == GuideStep.OreZone ||
                _currentStep == GuideStep.DepositZone)
            {
                int current = ResourceManager.Instance.GetAmount(ResourceType.Ore);
                if (current >= oreThreshold && _currentStep == GuideStep.OreZone)
                    SetStep(GuideStep.DepositZone);
            }
        }

        private void HandleOreDeposited(int count)
        {
            if (_currentStep != GuideStep.DepositZone) return;
            SetStep(GuideStep.HandcuffFactory);
        }

        private void HandleHandcuffReceived()
        {
            if (_currentStep != GuideStep.HandcuffFactory) return;
            SetStep(GuideStep.SalesZone);
        }

        private void HandleItemSold(int amount)
        {
            if (_currentStep != GuideStep.SalesZone) return;
            SetStep(GuideStep.CoinZone);
        }

        private void HandleCurrencyChanged(int balance)
        {
            if (_currentStep != GuideStep.CoinZone) return;
            if (balance <= 0) return;

            SetStep(GuideStep.Done);
        }

        // -------------------------------------------------------
        // 단계 설정
        // -------------------------------------------------------

        private void SetStep(GuideStep step)
        {
            _currentStep = step;
            HideAllIndicators();

            switch (step)
            {
                case GuideStep.OreZone:
                    // 존 위 화살표만 (방향 화살표 없음)
                    oreZoneIndicator?.SetActive(true);
                    _currentTarget = null;
                    directionArrow?.gameObject.SetActive(false);
                    break;

                case GuideStep.DepositZone:
                    // 존 위 화살표 + 방향 화살표
                    depositZoneIndicator?.SetActive(true);
                    _currentTarget = depositZoneTarget;
                    directionArrow?.gameObject.SetActive(true);
                    break;

                case GuideStep.HandcuffFactory:
                    // 존 위 화살표 + 방향 화살표
                    factoryIndicator?.SetActive(true);
                    _currentTarget = factoryTarget;
                    directionArrow?.gameObject.SetActive(true);
                    break;

                case GuideStep.SalesZone:
                    // 존 위 화살표만 (방향 화살표 없음)
                    salesZoneIndicator?.SetActive(true);
                    _currentTarget = null;
                    directionArrow?.gameObject.SetActive(false);
                    break;

                case GuideStep.CoinZone:
                    // 존 위 화살표만
                    coinZoneIndicator?.SetActive(true);
                    _currentTarget = null;
                    directionArrow?.gameObject.SetActive(false);
                    break;

                case GuideStep.Done:
                    _currentTarget = null;
                    directionArrow?.gameObject.SetActive(false);
                    break;
            }
        }

        private void HideAllIndicators()
        {
            oreZoneIndicator?.SetActive(false);
            depositZoneIndicator?.SetActive(false);
            factoryIndicator?.SetActive(false);
            salesZoneIndicator?.SetActive(false);
            coinZoneIndicator?.SetActive(false);
        }

        // -------------------------------------------------------
        // 방향 화살표
        // -------------------------------------------------------

        private void UpdateDirectionArrow()
        {
            if (_currentTarget == null || directionArrow == null) return;
            if (playerTransform == null) return;

            var dirToTarget = _currentTarget.position - playerTransform.position;
            dirToTarget.y = 0f;
            if (dirToTarget.magnitude < 0.1f) return;

            _bobTimer += Time.deltaTime * bobSpeed;
            directionArrow.position = playerTransform.position
                                    + Vector3.up * (arrowHeight
                                    + Mathf.Sin(_bobTimer) * bobHeight);

            directionArrow.rotation = Quaternion.LookRotation(dirToTarget.normalized);
        }
    }
}