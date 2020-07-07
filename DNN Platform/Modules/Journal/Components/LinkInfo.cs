// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class LinkInfo
    {
        public string URL { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<ImageInfo> Images { get; set; }
    }
}
