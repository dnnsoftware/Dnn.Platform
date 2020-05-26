// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.FileSystem.EventArgs
{
    public class FileAddedEventArgs : FileChangedEventArgs
    {
        public IFolderInfo FolderInfo { get; set; }
    }
}
