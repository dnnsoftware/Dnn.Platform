// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class DeleteItem
    {
        public bool IsFolder { get; set; }

        public int ItemId { get; set; }

        public string UnlinkAllowedStatus { get; set; }
    }
}
