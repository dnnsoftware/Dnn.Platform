#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
