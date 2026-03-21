namespace PlayableAd.Interface
{


    public interface IProcessable
    {
        ResourceType InputType { get; }
        ResourceType OutputType { get; }
        int InputRequired {  get; }
        int OutputRequired { get; }

        bool CanProcess(IResourceContainer container);
        void Process(IResourceContainer container);

    }

}