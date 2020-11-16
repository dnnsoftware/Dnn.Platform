// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.Caching;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    public abstract class BaseSettingsTests
    {
        public readonly object[] SettingsCases =
        {
            new object[] { "AbcdeF#2@kfdfdfds", 9, 1.45, false, new DateTime(2015, 11, 30, 13, 45, 16), TimeSpan.Zero, TestingEnum.Value1, default(ComplexType), },
            new object[] { "Bsskk41233[]#%&", -5, -13456.456, true, DateTime.Today.AddDays(-1), new TimeSpan(1, 5, 6, 7), TestingEnum.Value2, new ComplexType(8, -10), },
        };

        protected const string SettingNamePrefix = "UnitTestSetting_";
        protected const int ModuleId = 1234;
        protected const int TabModuleId = 653;
        protected const int TabId = 344597;
        protected const int PortalId = 246;

        protected MockRepository MockRepository;
        protected Hashtable MockCacheCollection;
        protected Mock<CachingProvider> MockCache;
        protected Mock<IHostController> MockHostController;
        protected Mock<IModuleController> MockModuleController;
        protected Mock<IPortalController> MockPortalController;

        public enum TestingEnum
        {
            Value1,
            Value2,
        }

        protected static ModuleInfo GetModuleInfo => new ModuleInfo { ModuleID = ModuleId, TabModuleID = TabModuleId, TabID = TabId, PortalID = PortalId, };

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            this.MockRepository = new MockRepository(MockBehavior.Default);
            this.MockHostController = this.MockRepository.Create<IHostController>();
        }

        [SetUp]
        public virtual void SetUp()
        {
            // Mock Repository and component factory
            this.MockRepository = new MockRepository(MockBehavior.Default);
            ComponentFactory.Container = new SimpleContainer();

            // Setup Mock
            this.MockCache = MockComponentProvider.CreateNew<CachingProvider>();
            HostController.RegisterInstance(this.MockHostController.Object);

            this.MockPortalController = this.MockRepository.Create<IPortalController>();
            PortalController.SetTestableInstance(this.MockPortalController.Object);
            this.MockModuleController = this.MockRepository.Create<IModuleController>();
            ModuleController.SetTestableInstance(this.MockModuleController.Object);

            // Setup mock cache
            this.MockCacheCollection = new Hashtable();
            this.MockHostController.Setup(hc => hc.GetString("PerformanceSetting")).Returns("3");
            this.MockCache.Setup(c => c.Insert(It.IsAny<string>(), It.IsAny<object>())).Callback((string cacheKey, object itemToCache) => this.MockCacheCollection[cacheKey] = itemToCache);
            this.MockCache.Setup(c => c.Insert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DNNCacheDependency>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>()))
                     .Callback((string cacheKey, object itemToCache, DNNCacheDependency dcd, DateTime dt, TimeSpan ts, CacheItemPriority cip, CacheItemRemovedCallback circ) => this.MockCacheCollection[cacheKey] = itemToCache);
            this.MockCache.Setup(c => c.GetItem(It.IsAny<string>())).Returns((string cacheKey) => this.MockCacheCollection[cacheKey]);
        }

        [TearDown]
        public virtual void TearDown()
        {
            MockComponentProvider.ResetContainer();
            PortalController.ClearInstance();
            ModuleController.ClearInstance();
        }

        protected static string CacheKey(ModuleInfo moduleInfo) => $"SettingsModule{moduleInfo.TabModuleID}";

        protected static string ModuleSettingsCacheKey(ModuleInfo moduleInfo) => $"ModuleSettings{moduleInfo.TabID}";

        protected static string TabModuleSettingsCacheKey(ModuleInfo moduleInfo) => $"TabModuleSettings{moduleInfo.TabID}";

        protected void MockPortalSettings(ModuleInfo moduleInfo, Dictionary<string, string> portalSettings)
        {
            this.MockPortalController.Setup(pc => pc.GetPortalSettings(moduleInfo.PortalID)).Returns(portalSettings);
        }

        protected void MockTabModuleSettings(ModuleInfo moduleInfo, Hashtable tabModuleSettings)
        {
            this.MockCache.Setup(c => c.GetItem("DNN_" + TabModuleSettingsCacheKey(moduleInfo)))
                .Returns(new Dictionary<int, Hashtable> { { moduleInfo.TabModuleID, tabModuleSettings } });
        }

        protected void MockModuleSettings(ModuleInfo moduleInfo, Hashtable moduleSettings)
        {
            this.MockCache.Setup(c => c.GetItem("DNN_" + ModuleSettingsCacheKey(moduleInfo)))
                .Returns(new Dictionary<int, Hashtable> { { moduleInfo.ModuleID, moduleSettings } });
        }
    }
}
