using System.Collections;
using UnityEngine;

namespace PlayableAd.Utils
{
    public static class ScalePopUtility
    {
        /// <summary>
        /// MonoBehaviour에서 코루틴으로 실행.
        /// 1.5 → 1.0 스케일 팝 애니메이션.
        /// </summary>
        public static IEnumerator ScalePop(
            GameObject go,
            float startScale = 1.5f,
            float duration = 0.25f)
        {
            if (go == null) yield break;

            var start = Vector3.one * startScale;
            var end = Vector3.one;
            var elapsed = 0f;

            go.transform.localScale = start;

            while (elapsed < duration)
            {
                if (go == null || !go.activeSelf) yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // easeOutBack
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                go.transform.localScale = Vector3.Lerp(start, end, easedT);

                yield return null;
            }

            if (go != null && go.activeSelf)
                go.transform.localScale = end;
        }
    }
}
