using PlayableAd.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Core
{
    ///<summary>
    ///이벤트 버스(옵저버 패턴)
    ///구독은 OnEnable, 해제는 OnDisable
    /// </summary>
    public static class GameEventBus
    {
        public static event Action<int> OnOreHarvested;
        public static event Action<int> OnHandcuffCrafted;

        public static event Action<int> OnItemSold;           
        public static event Action<int> OnCurrencyChanged;    
        public static event Action<int> OnCustomerServed;
        public static void Publish_OreHarvested(int amount) => OnOreHarvested?.Invoke(amount);
        public static void Publish_CurrencyChanged(int balance) => OnCurrencyChanged?.Invoke(balance);

        public static event Action<UpgradeType> OnUpgradePurchased;
        public static event Action<NPCRole, int> OnNPCSpawned;


        public static event Action<GameStage> OnStageChanged;
        public static event Action OnGameEnd;


        public static event Action<int> OnPrisonerAdded;   
        public static event Action OnPrisonFull;
        public static event Action OnPrisonExpanded;

        public static event Action OnCutsceneStart;
        public static event Action OnCutsceneEnd;

        public static event Action<int> OnOreDeposited;
        public static event Action OnHandcuffReceived;

        // Publish 메서드
        public static void Publish_HandcuffReceived() => OnHandcuffReceived?.Invoke();
        public static void Publish_OreDeposited(int count) => OnOreDeposited?.Invoke(count);
        public static void Publish_HandcuffCrafted(int amount) => OnHandcuffCrafted?.Invoke(amount);
        public static void Publish_ItemSold(int amount) => OnItemSold?.Invoke(amount);
        public static void Publish_UpgradePurchased(UpgradeType t) => OnUpgradePurchased?.Invoke(t);
        public static void Publish_NPCSpawned(NPCRole r, int count) => OnNPCSpawned?.Invoke(r, count);
        public static void Publish_StageChanged(GameStage stage) => OnStageChanged?.Invoke(stage);
        public static void Publish_PrisonFull() => OnPrisonFull?.Invoke();
        public static void Publish_PrisonExpanded() => OnPrisonExpanded?.Invoke();
        public static void Publish_GameEnd() => OnGameEnd?.Invoke();
        public static void Publish_CustomerServed(int totalCount) => OnCustomerServed?.Invoke(totalCount);
        public static void Publish_PrisonerAdded(int count) => OnPrisonerAdded?.Invoke(count);
        public static void Publish_CutsceneStart() => OnCutsceneStart?.Invoke();
        public static void Publish_CutsceneEnd() => OnCutsceneEnd?.Invoke();
    }

}