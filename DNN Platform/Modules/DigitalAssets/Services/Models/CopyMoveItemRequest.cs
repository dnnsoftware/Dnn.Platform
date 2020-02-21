// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class CopyMoveItemRequest
    {
        public int ItemId { get; set; }

        public int DestinationFolderId { get; set; }

        public bool Overwrite { get; set; }        
    }
}
