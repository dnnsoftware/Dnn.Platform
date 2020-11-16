// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class CopyMoveItemRequest
    {
        public int ItemId { get; set; }

        public int DestinationFolderId { get; set; }

        public bool Overwrite { get; set; }
    }
}
