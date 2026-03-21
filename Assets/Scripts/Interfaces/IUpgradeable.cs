namespace PlayableAd.Interface
{

    public interface IUpgradeable
    {
        UpgradeType UpgradeType { get; }
        bool IsUpgraded { get; }

        void ApplyUpgrade();
    }

}