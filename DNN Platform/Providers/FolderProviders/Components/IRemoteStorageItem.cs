// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Providers.FolderProviders.Components
{
    public interface IRemoteStorageItem
    {
        string Key { get; }
        DateTime LastModified { get; }
        long Size { get; }

        string HashCode { get; }
    }
}
