using UnityEngine;
using TMPro;
using PlayableAd.Core;

namespace PlayableAd.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("텍스트 UI")]
        [SerializeField] private TextMeshProUGUI currencyText;    // 돈
        [SerializeField] private TextMeshPro prisonerText;    // 판매 고객 수

        private void OnEnable()
        {
            GameEventBus.OnCurrencyChanged += HandleCurrencyChanged;
            GameEventBus.OnPrisonerAdded += HandlePrisonerAdded;
        }

        private void OnDisable()
        {
            GameEventBus.OnCurrencyChanged -= HandleCurrencyChanged;
            GameEventBus.OnPrisonerAdded -= HandlePrisonerAdded;
        }

        private void Start()
        {
            // 초기값 세팅
            UpdateCurrencyText(CurrencyManager.Instance.Balance);
            UpdatePrisonerText(
              PrisonManager.Instance.CurrentCount,
              PrisonManager.Instance.MaxCapacity);
        }

        private void HandleCurrencyChanged(int balance) => UpdateCurrencyText(balance);
        private void HandlePrisonerAdded(int currentCount)
               => UpdatePrisonerText(currentCount, PrisonManager.Instance.MaxCapacity);

        private void UpdateCurrencyText(int balance)
        {
            if (currencyText != null)
                currencyText.text = $"{balance}";
        }

        private void UpdatePrisonerText(int current, int max)
        {
            if (prisonerText != null)
            {
                prisonerText.text = $"{current}/{max}";
                prisonerText.color = current == max ? Color.red : Color.black;
            }


        }
    }
}