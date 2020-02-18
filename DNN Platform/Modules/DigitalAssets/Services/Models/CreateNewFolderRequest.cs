// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class CreateNewFolderRequest
    {
        public string FolderName { get; set; }

        public int ParentFolderId { get; set; }

        public int FolderMappingId { get; set; }

        public string MappedName { get; set; }
    }
}
