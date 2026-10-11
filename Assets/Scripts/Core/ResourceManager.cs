using PlayableAd.Interface;
using PlayableAd.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayableAd.Core
{
    public class ResourceManager : Singleton<ResourceManager>, IResourceContainer
    {
        //리소스 현재량
        private readonly Dictionary<ResourceType, int> _amounts = new()
        {
            { ResourceType.Ore, 0 },
            { ResourceType.Handcuff, 0 },
            { ResourceType.Coin, 0 },
        };

        //리소스 최대량
        private readonly Dictionary<ResourceType, int> _maxAmounts = new()
        {
            { ResourceType.Ore, 10 },
            {ResourceType.Handcuff, 20 },
            { ResourceType.Coin, 9999 },
        };

        protected override void OnAwake()
        {
            ServiceLocator.Register<IResourceContainer>(this);
        }

        public int GetAmount(ResourceType type)
        {
            return _amounts.TryGetValue(type, out var v) ? v : 0;
        }

        public bool TryAdd(ResourceType type, int amount)
        {
            if (!_amounts.ContainsKey(type)) return false;

            var current = _amounts[type];
            var max = _maxAmounts[type];

            if (current >= max) return false;

            _amounts[type] = Mathf.Min(current + amount, max);
            return true;
        }

        public bool TryRemove(ResourceType type, int amount)
        {
            if (!_amounts.ContainsKey(type)) return false;
            if (_amounts[type] < amount)return false;

            _amounts[type] -= amount;
            return true;
        }

        public int GetMax(ResourceType type)
        {
            return _maxAmounts.TryGetValue(type, out var v) ? v : 0;
        }

        public void SetMax(ResourceType type, int max)
        {
            if (_maxAmounts.ContainsKey(type))
                _maxAmounts[type] = max;
        }
    }
}
