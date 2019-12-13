// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    public class ZipExtractViewModel
    {
        public bool Ok { get; internal set; }

        public string ErrorMessage { get; internal set; }

        public ICollection<ExtractedItemViewModel> Items { get; internal set; }

        public IList<string> InvalidFiles { get; set; } 

        public int TotalCount { get; set; }
    }
}
