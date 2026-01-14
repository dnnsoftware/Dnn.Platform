// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Entities.Modules
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class BusinessControllerProviderTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    var fakeCachingProvider = new FakeCachingProvider(new Dictionary<string, object>(0));
                    ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);
                    services.AddSingleton<CachingProvider>(fakeCachingProvider);
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [TestCase(typeof(PortableControllerClass), true)]
        [TestCase(typeof(SearchableControllerClass), false)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetPortableInstanceFromType(Type businessControllerType, bool implementsPortable)
        {
            var provider = CreateProvider();
            var portable = provider.GetInstance<IPortable>(businessControllerType);

            if (implementsPortable)
            {
                Assert.That(portable, Is.Not.Null);
                Assert.That(portable, Is.InstanceOf<IPortable>());
            }
            else
            {
                Assert.That(portable, Is.Null);
            }
        }

        [TestCase(typeof(PortableControllerClass), true)]
        [TestCase(typeof(SearchableControllerClass), false)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetPortableInstanceFromTypeName(Type businessControllerType, bool implementsPortable)
        {
            var provider = CreateProvider();
            var portable = provider.GetInstance<IPortable>(businessControllerType.AssemblyQualifiedName);

            if (implementsPortable)
            {
                Assert.That(portable, Is.Not.Null);
                Assert.That(portable, Is.InstanceOf<IPortable>());
            }
            else
            {
                Assert.That(portable, Is.Null);
            }
        }

        [TestCase(typeof(PortableControllerClass), false)]
        [TestCase(typeof(SearchableControllerClass), true)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetSearchableInstanceFromType(Type businessControllerType, bool implementsSearchable)
        {
            var provider = CreateProvider();
            var searchable = provider.GetInstance<ModuleSearchBase>(businessControllerType);

            if (implementsSearchable)
            {
                Assert.That(searchable, Is.Not.Null);
                Assert.That(searchable, Is.InstanceOf<ModuleSearchBase>());
            }
            else
            {
                Assert.That(searchable, Is.Null);
            }
        }

        [TestCase(typeof(PortableControllerClass), false)]
        [TestCase(typeof(SearchableControllerClass), true)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetSearchableInstanceFromTypeName(Type businessControllerType, bool implementsSearchable)
        {
            var provider = CreateProvider();
            var searchable = provider.GetInstance<ModuleSearchBase>(businessControllerType.AssemblyQualifiedName);

            if (implementsSearchable)
            {
                Assert.That(searchable, Is.Not.Null);
                Assert.That(searchable, Is.InstanceOf<ModuleSearchBase>());
            }
            else
            {
                Assert.That(searchable, Is.Null);
            }
        }

        [TestCase(typeof(PortableControllerClass))]
        [TestCase(typeof(SearchableControllerClass))]
        [TestCase(typeof(AllFeaturesControllerClass))]
        public void CanGetInstanceFromType(Type businessControllerType)
        {
            var provider = CreateProvider();
            var controller = provider.GetInstance(businessControllerType);

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetType(), Is.EqualTo(businessControllerType));
        }

        [TestCase(typeof(PortableControllerClass))]
        [TestCase(typeof(SearchableControllerClass))]
        [TestCase(typeof(AllFeaturesControllerClass))]
        public void CanGetInstanceFromTypeName(Type businessControllerType)
        {
            var provider = CreateProvider();
            var controller = provider.GetInstance(businessControllerType.AssemblyQualifiedName);

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetType(), Is.EqualTo(businessControllerType));
        }

        private static BusinessControllerProvider CreateProvider()
        {
            return new BusinessControllerProvider(Globals.DependencyProvider);
        }

        private class PortableControllerClass : IPortable
        {
            public string ExportModule(int moduleID) => "Done";

            public void ImportModule(int moduleID, string content, string version, int userID)
            {
            }
        }

        private class SearchableControllerClass : ModuleSearchBase
        {
            public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
            {
                return Array.Empty<SearchDocument>();
            }
        }

        private class AllFeaturesControllerClass : ModuleSearchBase, IPortable, IUpgradeable
        {
            public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
            {
                return Array.Empty<SearchDocument>();
            }

            public string ExportModule(int moduleID) => $"Exported {moduleID}";

            public void ImportModule(int moduleID, string content, string version, int userID)
            {
            }

            public string UpgradeModule(string version) => $"Upgraded to {version}";
        }
    }
}
