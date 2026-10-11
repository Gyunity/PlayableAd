using PlayableAd.Utils;
using UnityEngine;

namespace PlayableAd.Items
{
    /// <summary>
    /// 시각적 코인 오브젝트.
    /// 실제 재화 계산은 CurrencyManager가 담당하고
    /// 이 클래스는 순수하게 시각 표현만 담당합니다.
    /// </summary>
    public class CoinItem : MonoBehaviour
    {
        public void ReturnToPool()
        {
            ObjectPool.Instance.Return(gameObject);
        }
    }

}