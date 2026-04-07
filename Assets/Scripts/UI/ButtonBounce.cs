using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PlayableAd.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonBounce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("평상시 반복 연출")]
        [SerializeField] private float idleStretchX = 1.15f; // 좌우 늘어남
        [SerializeField] private float idleSquishY = 0.9f;  // 위아래 줄어듦
        [SerializeField] private float idleSpeed = 1.5f;  // 반복 속도
        [SerializeField] private float idleDelay = 0.3f;  // 반복 사이 대기

        [Header("클릭 연출")]
        [SerializeField] private float stretchX = 1.3f;
        [SerializeField] private float squishY = 0.7f;
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private float bounceDuration = 0.4f;
        [SerializeField] private float overshoot = 1.1f;

        private Vector3 _originScale;
        private Coroutine _idleCoroutine;
        private Coroutine _clickCoroutine;
        private bool _isPressed;

        private void Awake()
        {
            _originScale = transform.localScale;
        }

        private void OnEnable()
        {
            _idleCoroutine = StartCoroutine(IdleRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _idleCoroutine = null;
            _clickCoroutine = null;
            _isPressed = false;
            transform.localScale = _originScale;
        }

        // -------------------------------------------------------
        // 평상시 반복 연출
        // -------------------------------------------------------

        private IEnumerator IdleRoutine()
        {
            while (true)
            {
                while (_isPressed)
                    yield return null;

                yield return StartCoroutine(ScaleTo(
                    new Vector3(
                        _originScale.x * idleStretchX,
                        _originScale.y * idleSquishY,
                        _originScale.z),
                    1f / idleSpeed * 0.4f,
                    EaseType.EaseOutQuad));

                while (_isPressed) yield return null;

                yield return StartCoroutine(ScaleTo(
                    _originScale,
                    1f / idleSpeed * 0.4f,
                    EaseType.EaseOutBack));

                while (_isPressed) yield return null;

                float elapsed = 0f;
                while (elapsed < idleDelay)
                {
                    if (_isPressed) break;
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        // -------------------------------------------------------
        // 클릭 연출
        // -------------------------------------------------------

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;

            if (_clickCoroutine != null) StopCoroutine(_clickCoroutine);
            _clickCoroutine = StartCoroutine(PressRoutine());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;

            if (_clickCoroutine != null) StopCoroutine(_clickCoroutine);
            _clickCoroutine = StartCoroutine(BounceRoutine());
        }

        private IEnumerator PressRoutine()
        {
            yield return StartCoroutine(ScaleTo(
                new Vector3(
                    _originScale.x * stretchX,
                    _originScale.y * squishY,
                    _originScale.z),
                pressDuration,
                EaseType.EaseOutQuad));
        }

        private IEnumerator BounceRoutine()
        {
            // 1단계: overshoot
            yield return StartCoroutine(ScaleTo(
                new Vector3(
                    _originScale.x * overshoot,
                    _originScale.y * (2f - overshoot),
                    _originScale.z),
                bounceDuration * 0.4f,
                EaseType.EaseOutCubic));

            // 2단계: 원래 크기 복귀
            yield return StartCoroutine(ScaleTo(
                _originScale,
                bounceDuration * 0.6f,
                EaseType.EaseOutCubic));

            // 복귀 완료 후 idle 재시작
            _idleCoroutine = StartCoroutine(IdleRoutine());
        }

        // -------------------------------------------------------
        // 공통 스케일 보간
        // -------------------------------------------------------

        private enum EaseType { EaseOutQuad, EaseOutCubic, EaseOutBack }

        private IEnumerator ScaleTo(Vector3 target, float duration, EaseType easeType)
        {
            float elapsed = 0f;
            var start = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float e = Ease(t, easeType);
                transform.localScale = Vector3.Lerp(start, target, e);
                yield return null;
            }

            transform.localScale = target;
        }

        private float Ease(float t, EaseType type)
        {
            switch (type)
            {
                case EaseType.EaseOutQuad:
                    return 1f - (1f - t) * (1f - t);
                case EaseType.EaseOutCubic:
                    return 1f - Mathf.Pow(1f - t, 3f);
                case EaseType.EaseOutBack:
                    float c1 = 1.70158f;
                    float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                default:
                    return t;
            }
        }
    }
}