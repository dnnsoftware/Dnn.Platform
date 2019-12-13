namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class CopyMoveItemRequest
    {
        public int ItemId { get; set; }

        public int DestinationFolderId { get; set; }

        public bool Overwrite { get; set; }        
    }
}
