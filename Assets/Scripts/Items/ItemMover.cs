using System;
using System.Collections;
using UnityEngine;

namespace PlayableAd.Items
{
    public class ItemMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float arcHeight = 1.2f;

        private Coroutine _moveCoroutine;

        public void MoveTo(Vector3 targetPos, Action onComplete = null)
        {
            if (_moveCoroutine != null)
                StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveRoutine(() => targetPos, onComplete));
        }

        public void MoveFromTo(Vector3 startPos, Vector3 targetPos, Action onComplete = null)
        {
            transform.position = startPos;
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            _moveCoroutine = StartCoroutine(MoveRoutine(() => targetPos, onComplete));
        }

        private IEnumerator MoveRoutine(Func<Vector3> getTarget, Action onComplete)
        {
            var startPos = transform.position;
            var elapsed = 0f;
            var duration = Mathf.Max(0.15f,
                Vector3.Distance(startPos, getTarget()) / moveSpeed);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var easedT = 1f - Mathf.Pow(1f - t, 3f);
                var current = getTarget();
                var flat = Vector3.Lerp(startPos, current, easedT);
                var arc = flat + Vector3.up * (arcHeight * Mathf.Sin(Mathf.PI * t));

                transform.position = arc;
                yield return null;
            }

            transform.position = getTarget();
            onComplete?.Invoke();
        }

        public void StopMove()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
        }
    }
}