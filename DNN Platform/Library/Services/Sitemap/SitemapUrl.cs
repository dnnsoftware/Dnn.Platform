#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Sitemap
{
    public class SitemapUrl
    {
        public string Url { get; set; }


        public DateTime LastModified { get; set; }


        public SitemapChangeFrequency ChangeFrequency { get; set; }


        public float Priority { get; set; }

        public List<AlternateUrl> AlternateUrls { get; set; }
    }

    public class AlternateUrl
    {
        public string Language { get; set; }

        public string Url { get; set; }
    }
}
