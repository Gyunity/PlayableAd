using PlayableAd.Utils;
using UnityEngine;

namespace PlayableAd.Core
{

    public class CurrencyManager : Singleton<CurrencyManager>
    {
        private int _balance;

        public int Balance => _balance;

        protected override void OnAwake()
        {
            ServiceLocator.Register<CurrencyManager>(this);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            _balance += amount;
            Debug.Log($"[CurrencyManager] Add({amount}) / ภÜพื: {_balance}\n{System.Environment.StackTrace}");

            GameEventBus.Publish_CurrencyChanged(_balance);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || _balance < amount) return false;
            _balance -= amount;
            GameEventBus.Publish_CurrencyChanged(_balance);
            return true;
        }
        public bool CanAfford(int amount) => _balance >= amount;
    }

}