#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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