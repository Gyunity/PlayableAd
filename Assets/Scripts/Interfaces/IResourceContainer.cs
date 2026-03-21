using PlayableAd.Interface;

namespace PlayableAd.Interface
{
    public interface IResourceContainer
    {
        int GetAmount(ResourceType type);
        bool TryAdd(ResourceType type, int amount);
        bool TryRemove(ResourceType type, int amount);
        int GetMax(ResourceType type);
        void SetMax(ResourceType type, int max);
        
    }
}