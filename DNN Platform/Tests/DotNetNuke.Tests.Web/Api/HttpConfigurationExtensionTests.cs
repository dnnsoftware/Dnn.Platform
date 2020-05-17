﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DotNetNuke.Web.Api;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class HttpConfigurationExtensionTests
    {
        [Test]
        public void GetTabAndModuleInfoProvidersReturnsEmptyWhenNoProvidersAdded()
        {
            //Arrange
            var configuration = new HttpConfiguration();

            //Act
            var providers = configuration.GetTabAndModuleInfoProviders();

            //Assert
            CollectionAssert.IsEmpty(providers);
        }

        [Test]
        public void AddTabAndModuleInfoProviderWorksForFirstProvider()
        {
            //Arrange
            var configuration = new HttpConfiguration();

            //Act
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());

            //Assert
            Assert.AreEqual(1, ((IEnumerable<ITabAndModuleInfoProvider>)configuration.Properties["TabAndModuleInfoProvider"]).Count());
        }

        [Test]
        public void AddTabAndModuleInfoProviderWorksForManyProviders()
        {
            //Arrange
            var configuration = new HttpConfiguration();

            //Act
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());

            //Assert
            Assert.AreEqual(3, ((IEnumerable<ITabAndModuleInfoProvider>)configuration.Properties["TabAndModuleInfoProvider"]).Count());
        }
    }
}
