namespace PlayableAd.Interface
{
    public interface IPurchasable
    {
        int Price { get; }
        bool IsPurchased { get; }

        bool CanPurchase(int currentCurrency);
        void Purchase();
    }

}