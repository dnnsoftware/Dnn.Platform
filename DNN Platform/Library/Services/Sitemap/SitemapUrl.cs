// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap
{
    using System;
    using System.Collections.Generic;

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
