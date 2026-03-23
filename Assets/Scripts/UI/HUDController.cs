using UnityEngine;
using TMPro;
using PlayableAd.Core;

namespace PlayableAd.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("텍스트 UI")]
        [SerializeField] private TextMeshProUGUI currencyText;    // 돈
        [SerializeField] private TextMeshPro customerText;    // 판매 고객 수

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

        private void Start()
        {
            // 초기값 세팅
            UpdateCurrencyText(CurrencyManager.Instance.Balance);
            UpdateCustomerText(0);
        }

        private void HandleCurrencyChanged(int balance) => UpdateCurrencyText(balance);
        private void HandleCustomerServed(int total) => UpdateCustomerText(total);

        private void UpdateCurrencyText(int balance)
        {
            if (currencyText != null)
                currencyText.text = $"{balance}";
        }

        private void UpdateCustomerText(int total)
        {
            if (customerText != null)
                customerText.text = $"{total}/ 20";
        }
    }
}