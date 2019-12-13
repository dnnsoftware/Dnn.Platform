using System.Collections.Generic;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    public class PageViewModel
    {
        public FolderViewModel Folder { get; set; }

        public ICollection<ItemViewModel> Items { get; set; }

        public int TotalCount { get; set; }
    }
}
