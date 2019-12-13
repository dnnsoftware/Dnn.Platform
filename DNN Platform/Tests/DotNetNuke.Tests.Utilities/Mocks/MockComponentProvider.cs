﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

using Moq;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Tests.Utilities.Mocks
{
    public class MockComponentProvider
    {
        public static Mock<T> CreateNew<T>() where T : class
        {
            if (ComponentFactory.Container == null)
            {
                ResetContainer();
            }

            //Try and get mock
            var mockComp = ComponentFactory.GetComponent<Mock<T>>();
            var objComp = ComponentFactory.GetComponent<T>();

            if (mockComp == null)
            {
                mockComp = new Mock<T>();
                ComponentFactory.RegisterComponentInstance<Mock<T>>(mockComp);
            }

            if (objComp == null)
            {
                ComponentFactory.RegisterComponentInstance<T>(mockComp.Object);
            }

            return mockComp;
        }

        public static Mock<T> CreateNew<T>(string name) where T : class
        {
            if (ComponentFactory.Container == null)
            {
                ResetContainer();
            }

            //Try and get mock
            var mockComp = ComponentFactory.GetComponent<Mock<T>>();
            var objComp = ComponentFactory.GetComponent<T>(name);

            if (mockComp == null)
            {
                mockComp = new Mock<T>();
                ComponentFactory.RegisterComponentInstance<Mock<T>>(mockComp);
            }

            if (objComp == null)
            {
                ComponentFactory.RegisterComponentInstance<T>(name, mockComp.Object);
            }

            return mockComp;
        }

        public static Mock<CachingProvider> CreateDataCacheProvider()
        {
            return CreateNew<CachingProvider>();
        }

        public static Mock<EventLogController> CreateEventLogController()
        {
            return CreateNew<EventLogController>();
        }

        public static Mock<DataProvider> CreateDataProvider()
        {
            return CreateNew<DataProvider>();
        }

        public static Mock<FolderProvider> CreateFolderProvider(string name)
        {
            return CreateNew<FolderProvider>(name);
        }

        public static Mock<ILocalizationProvider> CreateLocalizationProvider()
        {
            return CreateNew<ILocalizationProvider>();
        }

        public static Mock<ILocaleController> CreateLocaleController()
        {
            return CreateNew<ILocaleController>(); 
        }

        public static Mock<RoleProvider> CreateRoleProvider()
        {
            return CreateNew<RoleProvider>();
        }

        public static void ResetContainer()
        {
            ComponentFactory.Container = new SimpleContainer();
        }
    }
}
