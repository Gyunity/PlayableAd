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
        /// <summary>CriminalNPC가 수용소 입장 시 직접 호출</summary>
        public void AddPrisoner()
        {
            if (IsFull) return;

            CurrentCount++;
            GameEventBus.Publish_PrisonerAdded(CurrentCount);
            Debug.Log($"[PrisonManager] 수감자 추가: {CurrentCount}/{maxCapacity}");

            if (IsFull)
            {
                Debug.Log("[PrisonManager] 수용소 만석!");
                GameEventBus.Publish_StageChanged(GameStage.Stage8_PrisonExpand);
                GameEventBus.Publish_PrisonFull();
            }
        }
        public void ExpandCapacity(int amount)
        {
            maxCapacity += amount;
            GameEventBus.Publish_PrisonerAdded(CurrentCount);
            GameEventBus.Publish_StageChanged(GameStage.Stage9_End);

        }
    }

}