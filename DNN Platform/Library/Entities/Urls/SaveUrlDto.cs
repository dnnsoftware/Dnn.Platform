// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System;

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
