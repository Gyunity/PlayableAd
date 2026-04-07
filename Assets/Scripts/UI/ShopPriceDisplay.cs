using PlayableAd.Core;
using PlayableAd.Items;
using PlayableAd.Utils;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayableAd.UI
{
    public class ShopPriceDisplay : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject priceDisplay;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Image fillImage;    
        [SerializeField] private Transform coinTarget;

        [Header("설정")]
        [SerializeField] private int totalPrice;
        [SerializeField] private float coinInterval = 0.1f;
        [SerializeField] private string poolKey = "Coin";

        [Header("Fill 색상")]
        [SerializeField] private Color fillColor = new Color(0.2f, 0.8f, 0.2f, 0.6f); // 반투명 초록

        private int _remainingPrice;
        private bool _isProcessing;

        private void Awake()
        {
            _remainingPrice = totalPrice;

       
            if (fillImage != null)
            {
                fillImage.color = fillColor;
                fillImage.fillMethod = Image.FillMethod.Vertical;
                fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
                fillImage.fillAmount = 0f; // 시작은 비어있음
            }

            UpdatePriceText();
        }

        private void Start()
        {
            priceDisplay?.SetActive(true);
        }

        // -------------------------------------------------------
        // 지불 처리
        // -------------------------------------------------------

        public bool TryPay(Transform playerTransform)
        {
            if (_isProcessing) return false;
            if (_remainingPrice <= 0) return true;

            int balance = CurrencyManager.Instance.Balance;
            if (balance <= 0) return false;

            int payAmount = Mathf.Min(balance, _remainingPrice);
            StartCoroutine(PayRoutine(playerTransform, payAmount));

            return _remainingPrice - payAmount <= 0;
        }

        private IEnumerator PayRoutine(Transform playerTransform, int payAmount)
        {
            _isProcessing = true;

            int coinCount = payAmount / 10;

            for (int i = 0; i < coinCount; i++)
            {
                // 코인 포물선 이동
                var go = ObjectPool.Instance.Get(
                             poolKey,
                             playerTransform.position,
                             Quaternion.identity);

                if (go != null && go.TryGetComponent<ItemMover>(out var mover))
                    mover.MoveTo(coinTarget.position, onComplete: () =>
                        ObjectPool.Instance.Return(go));

                // 10원 차감
                CurrencyManager.Instance.TrySpend(10);
                _remainingPrice -= 10;
                _remainingPrice = Mathf.Max(0, _remainingPrice);

                UpdateFill();
                UpdatePriceText();
                SoundManager.Instance.PlayCoinPay();
                yield return new WaitForSeconds(coinInterval);
            }
            if (_remainingPrice <= 0)
                SoundManager.Instance.PlayPurchaseDone();
            _isProcessing = false;
        }

        // -------------------------------------------------------
        // UI 업데이트
        // -------------------------------------------------------

        private void UpdateFill()
        {
            if (fillImage == null) return;

            // 납부 비율 = (전체 가격 - 남은 가격) / 전체 가격
            float paidRatio = 1f - (float)_remainingPrice / totalPrice;

            StopCoroutine(nameof(AnimateFill));
            StartCoroutine(AnimateFill(paidRatio));
        }

        private IEnumerator AnimateFill(float targetFill)
        {
            float startFill = fillImage.fillAmount;
            float elapsed = 0f;
            float duration = 0.15f; // 빠르게 채워짐

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fillImage.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / duration);
                yield return null;
            }

            fillImage.fillAmount = targetFill;

            if (targetFill >= 1f)
                fillImage.color = new Color(0.1f, 0.9f, 0.1f, 0.8f); // 더 진한 초록
        }

        private void UpdatePriceText()
        {
            if (priceText == null) return;

            if (_remainingPrice <= 0)
            {
                priceText.text = "0";
                priceText.color = Color.white;
            }
            else
            {
                priceText.text = _remainingPrice.ToString();
                priceText.color = Color.white;
            }
        }

        public bool IsPaid => _remainingPrice <= 0;
        public int Remaining => _remainingPrice;

        public void ResetPrice()
        {
            _remainingPrice = totalPrice;
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
                fillImage.color = fillColor;
            }
            UpdatePriceText();
        }
    }
}