// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models
{
    using System.Collections.Generic;

    public class PageViewModel
    {
        public FolderViewModel Folder { get; set; }

        public ICollection<ItemViewModel> Items { get; set; }

        public int TotalCount { get; set; }
    }
}
