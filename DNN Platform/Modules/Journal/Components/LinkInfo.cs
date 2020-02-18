// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Journal.Components {
    public class LinkInfo {
        public string URL { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ImageInfo> Images { get; set; }

    }
}
