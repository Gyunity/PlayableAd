namespace PlayableAd.Interface
{


    public interface ISellable
    {
        ResourceType SellResourceType { get; }
        int SellAmountPerCustomer { get; }

        bool CanSell(IResourceContainer container);
    }

}