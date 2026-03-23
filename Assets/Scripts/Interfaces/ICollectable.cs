using UnityEngine;

namespace PlayableAd.Interface
{
    public interface ICollectable
    {
        bool IsCollectable { get; }
        ResourceType ResourceType { get; }
        int CollectAmount { get; }

        bool Collect(GameObject collector);
        void OnCollected();
        void ResetCollectable();
        void SetCollectable(bool value);
    }

}