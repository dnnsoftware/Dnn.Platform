// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Seo.Services.Dto
{
    public class SitemapSettingsRequest
    {
        public string SitemapUrl { get; set; }

        public bool SitemapLevelMode { get; set; }

        public float SitemapMinPriority { get; set; }

        public bool SitemapIncludeHidden { get; set; }

        public float SitemapExcludePriority { get; set; }

        public int SitemapCacheDays { get; set; }
    }
}
