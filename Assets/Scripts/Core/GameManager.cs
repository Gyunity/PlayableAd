using PlayableAd.Interface;
using PlayableAd.Utils;
using System.Xml.Serialization;
using UnityEngine;

namespace PlayableAd.Core
{

    /// <summary>
    /// 게임 단계 별 조건을 관리
    /// 연출은 StageController에 위임
    /// </summary>

    public class GameManager : Singleton<GameManager>
    {
        public GameStage CurrentStage { get; private set; } = GameStage.Stage1_Mining;

        protected override void OnAwake()
        {
            ServiceLocator.Register<GameManager>(this);
        }

        private void OnEnable()
        {
            GameEventBus.OnCurrencyChanged += HandleCurrencyChanged;
            GameEventBus.OnUpgradePurchased += HandleUpgrade;
        }

        private void OnDisable()
        {
            GameEventBus.OnCurrencyChanged -= HandleCurrencyChanged;
            GameEventBus.OnUpgradePurchased -= HandleUpgrade;
        }

        //다음 시테이지 조건 체크
        private void HandleCurrencyChanged(int balance)
        {
            if (CurrentStage == GameStage.Stage3_Sales && balance >= 100)
                AdvanceTo(GameStage.Stage4_WeaponUpgrade);
        }
        private void HandleUpgrade(UpgradeType type)
        {
            var next = type switch
            {
                UpgradeType.Weapon1 => GameStage.Stage5_BuyMiner,
                UpgradeType.Miner => GameStage.Stage6_BuyWorker,
                UpgradeType.Worker => GameStage.Stage7_WeaponUpgrade2,
                //UpgradeType.Weapon2 => GameStage.Stage8_PrisonExpand,
                UpgradeType.PrisonExpand => GameStage.Stage9_End,
                _ => CurrentStage
            };

            if (next != CurrentStage)
                AdvanceTo(next);
        }
        public void AdvanceTo(GameStage stage)
        {
            if (stage <= CurrentStage) return;
            CurrentStage = stage;
            Debug.Log($"[GameManager] 스테이지 전환 → {stage}");
            GameEventBus.Publish_StageChanged(stage);

            if(stage == GameStage.Stage9_End)
                GameEventBus.Publish_GameEnd();
        }
    }

}