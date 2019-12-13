namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class DeleteItem
    {
        public bool IsFolder { get; set; }

        public int ItemId { get; set; }

        public string UnlinkAllowedStatus { get; set; }
    }
}
