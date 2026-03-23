using UnityEngine;
using PlayableAd.Utils;
using PlayableAd.Interface;

namespace PlayableAd.Core
{
    /// <summary>
    /// 수용소 인원을 관리합니다.
    /// 판매 완료 시 수감자 +1, Max 도달 시 판매 잠금.
    /// </summary>

    public class PrisonManager : Singleton<PrisonManager>
    {
        [SerializeField]
        private int maxCapacity = 20;

        public int CurrentCount { get; private set; } = 0;
        public int MaxCapacity => maxCapacity;
        public bool IsFull => CurrentCount >= maxCapacity;

        protected override void OnAwake()
        {
            ServiceLocator.Register<PrisonManager>(this);
        }

        private void OnEnable()
        {
            GameEventBus.OnCustomerServed += HandleCustomerServed;
        }

        private void OnDisable()
        {
            GameEventBus.OnCustomerServed -= HandleCustomerServed;
        }

        private void HandleCustomerServed(int totalCount)
        {
            if (IsFull) return;

            CurrentCount++;
            GameEventBus.Publish_PrisonerAdded(CurrentCount);

            if (IsFull)
            {
                Debug.Log("수용소 꽉참");
                GameManager.Instance.AdvanceTo(GameStage.Stage8_PrisonExpand);
                GameEventBus.Publish_PrisonFull();
            }
        }

        public void ExpandCapacity(int amount)
        {
            maxCapacity += amount;
            GameEventBus.Publish_PrisonExpanded();
        }
    }

}