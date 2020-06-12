// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
