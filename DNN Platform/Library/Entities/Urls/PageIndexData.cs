using System;

namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// The PageIndexData class is used during the page index build process.
    /// </summary>
    [Serializable]
    internal class PageIndexData
    {
        public string LastPageKey;
        public string LastPageValue;
    }
}
