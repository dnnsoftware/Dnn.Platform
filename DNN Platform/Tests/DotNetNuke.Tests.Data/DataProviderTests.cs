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

using System;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Tests.Data.Fakes;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Data
{
    [TestFixture]
    public class DataProviderTests
    {
        [Test]
        public void DataProvider_Instance_Method_Returns_Instance()
        {
            //Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(new Dictionary<string, string>()));

            //Act
            var provider = DataProvider.Instance();

            //Assert
            Assert.IsInstanceOf<DataProvider>(provider);
            Assert.IsInstanceOf<FakeDataProvider>(provider);
        }

        [Test]
        public void DataProvider_ConnectionString_Property_Is_Valid()
        {
            //Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(new Dictionary<string, string>()));

            var connectionString = Config.GetConnectionString();

            //Act
            var provider = DataProvider.Instance();

            //Assert
            Assert.AreEqual(connectionString, provider.ConnectionString);
        }

        [Test]
        [TestCase("")]
        [TestCase("dbo.")]
        public void DataProvider_DatabaseOwner_Property_Is_Valid(string databaseOwner)
        {
            //Arrange
            var settings = new Dictionary<string, string>();
            settings["databaseOwner"] = databaseOwner;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            //Act
            var provider = DataProvider.Instance();

            //Assert
            Assert.AreEqual(databaseOwner, provider.DatabaseOwner);
        }

        [Test]
        [TestCase("")]
        [TestCase("dnn_")]
        public void DataProvider_ObjectQualifier_Property_Is_Valid(string objectQualifier)
        {
            //Arrange
            var settings = new Dictionary<string, string>();
            settings["objectQualifier"] = objectQualifier;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            //Act
            var provider = DataProvider.Instance();

            //Assert
            Assert.AreEqual(objectQualifier, provider.ObjectQualifier);
        }

        [Test]
        [TestCase("SqlDataProvider")]
        [TestCase("FakeDataProvider")]
        public void DataProvider_ProviderName_Property_Is_Valid(string providerName)
        {
            //Arrange
            var settings = new Dictionary<string, string>();
            settings["providerName"] = providerName;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            //Act
            var provider = DataProvider.Instance();

            //Assert
            Assert.AreEqual(providerName, provider.ProviderName);
        }

        [Test]
        [TestCase("somePath")]
        [TestCase("someOtherPath")]
        public void DataProvider_ProviderPath_Property_Is_Valid(string providerPath)
        {
            //Arrange
            var settings = new Dictionary<string, string>();
            settings["providerPath"] = providerPath;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            //Act
            var provider = DataProvider.Instance();

            //Assert
            Assert.AreEqual(providerPath, provider.ProviderPath);
        }

    }
}
