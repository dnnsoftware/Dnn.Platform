// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class RenameFolderRequest
    {
        public int FolderId { get; set; }

        public string NewFolderName { get; set; }
    }
}
