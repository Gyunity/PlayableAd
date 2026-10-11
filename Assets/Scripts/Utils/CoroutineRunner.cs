using System.Collections;
using UnityEngine;

namespace PlayableAd.Utils
{
    /// <summary>
    /// MonoBehaviour 없는 곳에서 코루틴을 실행할 수 있게 해주는 유틸.
    /// </summary>
    public class CoroutineRunner : Singleton<CoroutineRunner>
    {
        public void Run(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}