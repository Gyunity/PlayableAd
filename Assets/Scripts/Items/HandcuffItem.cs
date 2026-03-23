using UnityEngine;
using PlayableAd.Interface;
using PlayableAd.Core;
using PlayableAd.Utils;

namespace PlayableAd.Items
{

    public class HandcuffItem : MonoBehaviour, ICollectable
    {
        public bool IsCollectable { get; private set; } = true;
        public ResourceType ResourceType => ResourceType.Handcuff;
        public int CollectAmount => 1;

        public bool Collect(GameObject collector)
        {
            if (!IsCollectable)
                return false;
            if (!ResourceManager.Instance.TryAdd(ResourceType, CollectAmount))
                return false;

            IsCollectable = false;
            OnCollected();
            return true;
        }

        public void OnCollected()
        {
            GameEventBus.Publish_HandcuffCrafted(CollectAmount);
            ObjectPool.Instance.Return(gameObject);
        }

        public void ResetCollectable()
        {
            IsCollectable = true;
        }

        public void SetCollectable(bool value)
        {
            IsCollectable = value;
        }
    }

}