using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class SynchronizeFolderRequest
    {
        public int FolderId { get; set; }

        public bool Recursive { get; set; }
    }
}