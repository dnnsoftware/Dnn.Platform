namespace DotNetNuke.Entities.Modules.Communications
{
    public interface IModuleCommunicator
    {
        event ModuleCommunicationEventHandler ModuleCommunication;
    }
}
