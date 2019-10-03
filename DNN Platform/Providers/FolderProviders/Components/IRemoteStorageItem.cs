#region Copyright
// DotNetNuke® - https://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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
