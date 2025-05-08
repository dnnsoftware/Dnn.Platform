// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Security.Permissions;

using System.Collections.Generic;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Tests.Utilities.Fakes;

using Microsoft.Extensions.DependencyInjection;

using Moq;
using NUnit.Framework;

[TestFixture]
public class PermissionProviderTests
{
    [Test]
    public void SaveFolderPermissions_DoesNotThrow()
    {
        var permissions = new List<PermissionInfo>();
        var cache = new Dictionary<string, object> { { CachingProvider.GetCacheKey(DataCache.PermissionsCacheKey), permissions }, };
        var fakeCachingProvider = new FakeCachingProvider(cache);
        ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);

        ComponentFactory.RegisterComponentInstance<DataProvider>(Mock.Of<DataProvider>());
        using var serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));

        var permissionProvider = new PermissionProvider();

        IFolderInfo folder = new FolderInfo(initialiseEmptyPermissions: true);
        Assert.DoesNotThrow(() => permissionProvider.SaveFolderPermissions(folder));
    }

    [Test]
    public void SaveFolderPermissions_WithPermissions_DoesNotThrow()
    {
        var readPermission = new PermissionInfo { PermissionCode = "SYSTEM_FOLDER", PermissionKey = "READ", PermissionID = 1, };
        var permissions = new List<PermissionInfo> { readPermission, };
        var cache = new Dictionary<string, object> { { CachingProvider.GetCacheKey(DataCache.PermissionsCacheKey), permissions }, };
        var fakeCachingProvider = new FakeCachingProvider(cache);
        ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);

        ComponentFactory.RegisterComponentInstance<DataProvider>(Mock.Of<DataProvider>());
        using var serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));

        var permissionProvider = new PermissionProvider();

        IFolderInfo folder = new FolderInfo(initialiseEmptyPermissions: true);
        folder.FolderPermissions.Add(new FolderPermissionInfo(readPermission) { AllowAccess = true, RoleID = -2, UserID = -1, FolderID = 2, });
        Assert.DoesNotThrow(() => permissionProvider.SaveFolderPermissions(folder));
    }
}
