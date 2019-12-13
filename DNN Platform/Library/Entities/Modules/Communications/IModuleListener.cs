namespace DotNetNuke.Entities.Modules.Communications
{
    public interface IModuleListener
    {
        void OnModuleCommunication(object s, ModuleCommunicationEventArgs e);
    }
}
