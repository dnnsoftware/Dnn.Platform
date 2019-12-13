namespace DotNetNuke.Entities.Modules.Communications
{
    public class RoleChangeEventArgs : ModuleCommunicationEventArgs
    {
        public string PortalId { get; set; }

        public string RoleId { get; set; }
    }
}
