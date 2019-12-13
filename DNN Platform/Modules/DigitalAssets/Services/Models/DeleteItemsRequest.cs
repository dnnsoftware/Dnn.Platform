using System.Collections.Generic;

namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class DeleteItemsRequest
    {
        public IEnumerable<DeleteItem> Items { get; set; }
    }
}
