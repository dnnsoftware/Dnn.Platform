namespace DotNetNuke.Entities.Portals
{
    public class UserCopiedEventArgs
    {
        public bool Cancel { get; set; }
        public string PortalName { get; set; }
        public float TotalUsers { get; set; }
        public string UserName { get; set; }
        public float UserNo { get; set; }
        public string Stage { get; set; }
        public int PortalGroupId { get; set; }
    }
}
