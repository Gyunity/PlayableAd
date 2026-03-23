using PlayableAd.Core;
using PlayableAd.Interface;
using UnityEngine;

namespace PlayableAd.Systems
{
    /// <summary>
    /// 판매 이벤트를 수신해서 전체 판매 상태를 관리합니다.
    /// 고객 수 누적 카운팅 및 스테이지 전환 조건을 체크합니다.
    /// </summary>
    public class SalesSystem : MonoBehaviour
    {
        [Header("Stage4 전환 조건")]
        [SerializeField] private int targetCurrency = 100; // 100원 달성 시 Stage4

        private bool _stage4Triggered = false;

        private void OnEnable()
        {
            GameEventBus.OnCurrencyChanged += HandleCurrencyChanged;
            GameEventBus.OnCustomerServed += HandleCustomerServed;
        }

        private void OnDisable()
        {
            GameEventBus.OnCurrencyChanged -= HandleCurrencyChanged;
            GameEventBus.OnCustomerServed -= HandleCustomerServed;
        }

        private void HandleCustomerServed(int totalCount)
        {
            Debug.Log($"[SalesSystem] 누적 고객 수: {totalCount}명 " +
                      $"/ 잔액: {CurrencyManager.Instance.Balance}원");
        }

        private void HandleCurrencyChanged(int balance)
        {
            // 최초 100원 달성 시 1회만 Stage4 전환
            if (_stage4Triggered) return;
            if (balance < targetCurrency) return;

            _stage4Triggered = true;
            Debug.Log($"[SalesSystem] 목표 금액 {targetCurrency}원 달성 → Stage4 전환");
            GameManager.Instance.AdvanceTo(GameStage.Stage4_WeaponUpgrade);
        }
    }
}