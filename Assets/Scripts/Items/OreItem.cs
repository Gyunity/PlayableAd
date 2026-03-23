using PlayableAd.Core;
using PlayableAd.Interface;
using PlayableAd.Systems;
using PlayableAd.Utils;
using System;
using UnityEngine;

namespace PlayableAd.Items
{
    [RequireComponent(typeof(Collider))]
    public class OreItem : MonoBehaviour, ICollectable
    {
        //ICollectable
        public bool IsCollectable { get; private set; } = true;
        public ResourceType ResourceType => ResourceType.Ore;
        public int CollectAmount => 1;


        public Action OnCollectedCallback { get; set; }

        public bool Collect(GameObject collector)
        {
            if (!IsCollectable) return false;
            if (ResourceManager.Instance == null) return false;
            if (!ResourceManager.Instance.TryAdd(ResourceType, CollectAmount))
                return false;

            IsCollectable = false;
            OnCollected();
            return true;
        }
        public void HarvestByNPC()
        {
            if (!IsCollectable) return;

            IsCollectable = false;

            // OreZone 리스폰 콜백 실행 (채굴 후 리스폰 타이머 시작)
            OnCollectedCallback?.Invoke();
            OnCollectedCallback = null;
        }
        public void OnCollected()
        {
            GameEventBus.Publish_OreHarvested(CollectAmount);

            OnCollectedCallback?.Invoke();
            ObjectPool.Instance.Return(gameObject);

        }

        public void ResetCollectable()
        {
            IsCollectable = true;
            OnCollectedCallback = null;

        }
        public void SetCollectable(bool value)
        {
            IsCollectable = value;
        }
    }
}