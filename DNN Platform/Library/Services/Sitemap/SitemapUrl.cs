// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
