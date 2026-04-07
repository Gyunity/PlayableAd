
using System.Collections;
using UnityEngine;

namespace PlayableAd.UI
{

    public class MaxIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject maxImageObj;
        [SerializeField] private Vector3 offset = new(0f, 1.5f, 0f);

        [Header("팝업 연출 설정")]
        [SerializeField] private float riseHeight = 0.8f;
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private float holdTime = 0.2f;

        private Camera _cam;
        private Coroutine _popCoroutine;

        private Vector3 _currentOffset;
        private bool _isAnimating;

        private void Awake()
        {
            _cam = Camera.main;
            _currentOffset = offset;
            Hide();
        }

        private void LateUpdate()
        {
            if (maxImageObj == null || !maxImageObj.activeSelf) return;

            maxImageObj.transform.position = transform.position + _currentOffset;

            maxImageObj.transform.LookAt(
                maxImageObj.transform.position + _cam.transform.forward);
        }

        public void Show()
        {
            if (maxImageObj == null) return;

            if (_popCoroutine != null)
                StopCoroutine(_popCoroutine);

            _popCoroutine = StartCoroutine(PopRoutine());
        }

        public void Hide()
        {
            if (_popCoroutine != null)
            {
                StopCoroutine(_popCoroutine);
                _popCoroutine = null;
            }

            _currentOffset = offset;
            _isAnimating = false;

            if (maxImageObj != null)
            {
                maxImageObj.SetActive(false);
                SetAlpha(1f);
            }
        }

        private IEnumerator PopRoutine()
        {
            _isAnimating = true;
            _currentOffset = offset; // 시작 오프셋 초기화
            SetAlpha(1f);
            maxImageObj.SetActive(true);

            float elapsed = 0f;
            while (elapsed < holdTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            float fadeDuration = duration - holdTime;
            var startOffset = offset;
            var endOffset = offset + Vector3.up * riseHeight;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                _currentOffset = Vector3.Lerp(startOffset, endOffset, t);

                // 서서히 투명해짐
                SetAlpha(1f - t);

                yield return null;
            }

            maxImageObj.SetActive(false);
            _currentOffset = offset;
            _isAnimating = false;
            _popCoroutine = null;
        }

        // -------------------------------------------------------
        // 투명도 제어
        // -------------------------------------------------------

        private void SetAlpha(float alpha)
        {
            if (maxImageObj == null) return;

            // MeshRenderer (Quad 방식)
            if (maxImageObj.TryGetComponent<Renderer>(out var rend))
            {
                var mat = rend.material;
                var color = mat.color;
                color.a = alpha;
                mat.color = color;
                return;
            }

            // CanvasGroup (UI 방식)
            if (maxImageObj.TryGetComponent<CanvasGroup>(out var cg))
                cg.alpha = alpha;
        }

        public bool IsShowing => maxImageObj != null && maxImageObj.activeSelf;
    }
}


