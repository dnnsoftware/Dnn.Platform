// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    public abstract class BaseSettingsTests
    {
        protected const string SettingNamePrefix = "UnitTestSetting_";
        protected const int ModuleId = 1234;
        protected const int TabModuleId = 653;
        protected const int TabId = 344597;
        protected const int PortalId = 246;

        protected MockRepository MockRepository;
        protected Mock<CachingProvider> MockCache;
        protected Mock<IHostController> MockHostController;

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
            MockRepository = new MockRepository(MockBehavior.Default);
            MockHostController = MockRepository.Create<IHostController>();
        }

        [SetUp]
        public virtual  void SetUp()
        {
            //Mock Repository and component factory
            MockRepository = new MockRepository(MockBehavior.Default);
            ComponentFactory.Container = new SimpleContainer();

            // Setup Mock
            MockCache = MockComponentProvider.CreateNew<CachingProvider>();
            HostController.RegisterInstance(MockHostController.Object);
        }

        [TearDown]
        public virtual void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        public enum TestingEnum
        {
            Value1,
            Value2
        }

        public readonly object[] SettingsCases =
        {
            new object[] { "AbcdeF#2@kfdfdfds", 9, 1.45, false, new DateTime(2015, 11, 30, 13, 45, 16), TimeSpan.Zero, TestingEnum.Value1, new ComplexType(), },
            new object[] { "Bsskk41233[]#%&", -5, -13456.456, true, DateTime.Today.AddDays(-1), new TimeSpan(1,5,6,7), TestingEnum.Value2, new ComplexType(8, -10), },
        };

        protected static ModuleInfo GetModuleInfo => new ModuleInfo { ModuleID = ModuleId, TabModuleID = TabModuleId, TabID = TabId, PortalID = PortalId, };

        protected static string CacheKey(ModuleInfo moduleInfo) => $"SettingsModule{moduleInfo.TabModuleID}";

        protected static string ModuleSettingsCacheKey(ModuleInfo moduleInfo) => $"ModuleSettings{moduleInfo.TabID}";

        protected static string TabModuleSettingsCacheKey(ModuleInfo moduleInfo) => $"TabModuleSettings{moduleInfo.TabID}";
    }
}
