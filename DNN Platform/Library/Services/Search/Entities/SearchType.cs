using System;

namespace DotNetNuke.Services.Search.Entities
{
    [Serializable]
    public class SearchType
    {
        /// <summary>
        /// Search Type Id
        /// </summary>
        public int SearchTypeId { get; set; }
        
        /// <summary>
        /// Search Type Name
        /// </summary>
        public string SearchTypeName { get; set; }

        /// <summary>
        /// A class implementing BaseResultController. This class will be invoked by reflection.
        /// </summary>
        public string SearchResultClass { get; set; }

        /// <summary>
        /// Content from this SearchType will normally be not searched while performing site or module search
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}
