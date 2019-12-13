// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class DeleteItemsRequest
    {
        public IEnumerable<DeleteItem> Items { get; set; }
    }
}
