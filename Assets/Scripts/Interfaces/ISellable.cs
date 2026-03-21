namespace PlayableAd.Interface
{


    public interface ISellable
    {
        ResourceType SellResourceType { get; }
        int SellAmountPerCustomer { get; }
        int RevenusPerCustomer {  get; }

        bool CanSell(IResourceContainer container);
        int Sell(IResourceContainer container);
    }

}