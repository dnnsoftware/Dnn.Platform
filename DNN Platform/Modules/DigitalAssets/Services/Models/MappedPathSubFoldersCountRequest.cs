// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;

namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class MappedPathSubFoldersCountRequest
    {
        public IEnumerable<ItemBaseViewModel> Items { get; set; }
    }
}
