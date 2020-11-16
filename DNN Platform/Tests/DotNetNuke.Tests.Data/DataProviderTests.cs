// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Tests.Data.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DataProviderTests
    {
        [Test]
        public void DataProvider_Instance_Method_Returns_Instance()
        {
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(new Dictionary<string, string>()));

            // Act
            var provider = DataProvider.Instance();

            // Assert
            Assert.IsInstanceOf<DataProvider>(provider);
            Assert.IsInstanceOf<FakeDataProvider>(provider);
        }

        [Test]
        public void DataProvider_ConnectionString_Property_Is_Valid()
        {
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(new Dictionary<string, string>()));

            var connectionString = Config.GetConnectionString();

            // Act
            var provider = DataProvider.Instance();

            // Assert
            Assert.AreEqual(connectionString, provider.ConnectionString);
        }

        [Test]
        [TestCase("")]
        [TestCase("dbo.")]
        public void DataProvider_DatabaseOwner_Property_Is_Valid(string databaseOwner)
        {
            // Arrange
            var settings = new Dictionary<string, string>();
            settings["databaseOwner"] = databaseOwner;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();

            // Assert
            Assert.AreEqual(databaseOwner, provider.DatabaseOwner);
        }

        [Test]
        [TestCase("")]
        [TestCase("dnn_")]
        public void DataProvider_ObjectQualifier_Property_Is_Valid(string objectQualifier)
        {
            // Arrange
            var settings = new Dictionary<string, string>();
            settings["objectQualifier"] = objectQualifier;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();

            // Assert
            Assert.AreEqual(objectQualifier, provider.ObjectQualifier);
        }

        [Test]
        [TestCase("SqlDataProvider")]
        [TestCase("FakeDataProvider")]
        public void DataProvider_ProviderName_Property_Is_Valid(string providerName)
        {
            // Arrange
            var settings = new Dictionary<string, string>();
            settings["providerName"] = providerName;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();

            // Assert
            Assert.AreEqual(providerName, provider.ProviderName);
        }

        [Test]
        [TestCase("somePath")]
        [TestCase("someOtherPath")]
        public void DataProvider_ProviderPath_Property_Is_Valid(string providerPath)
        {
            // Arrange
            var settings = new Dictionary<string, string>();
            settings["providerPath"] = providerPath;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();

            // Assert
            Assert.AreEqual(providerPath, provider.ProviderPath);
        }
    }
}
