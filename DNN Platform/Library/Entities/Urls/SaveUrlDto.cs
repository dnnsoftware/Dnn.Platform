// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Urls
{
    public class SaveUrlDto
    {
        public int Id { get; set; }
        public int SiteAliasKey { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public int LocaleKey { get; set; }
        public int StatusCodeKey { get; set; }
        public int SiteAliasUsage { get; set; }
        public bool IsSystem { get; set; }
    }

}
