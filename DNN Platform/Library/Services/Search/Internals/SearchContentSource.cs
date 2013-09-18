using System;

using DotNetNuke.Services.Search.Entities;

namespace DotNetNuke.Services.Search.Internals
{
    [Serializable]
    public class SearchContentSource: SearchType
    {
        public string LocalizedName { get; set; }

        public int ModuleDefinitionId { get; set; }
    }
}
