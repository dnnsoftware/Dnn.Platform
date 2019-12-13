using System.Collections.Generic;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;

namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class MappedPathSubFoldersCountRequest
    {
        public IEnumerable<ItemBaseViewModel> Items { get; set; }
    }
}
