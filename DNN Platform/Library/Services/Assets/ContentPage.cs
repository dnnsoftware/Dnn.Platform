using System.Collections.Generic;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.Assets
{
    public class ContentPage
    {
        public IFolderInfo Folder { get; set; }

        public ICollection<object> Items { get; set; }

        public int TotalCount { get; set; }
    }
}
