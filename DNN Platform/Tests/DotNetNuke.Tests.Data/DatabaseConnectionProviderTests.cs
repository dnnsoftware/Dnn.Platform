// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Tests.Data.Fakes;
    using NUnit.Framework;

    [TestFixture]
    public class DatabaseConnectionProviderTests
    {
        [Test]
        public void DatabaseConnectionProvider_Instance_Method_Returns_Instance()
        {
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DatabaseConnectionProvider>(new FakeDbConnectionProvider());

            // Act
            var provider = DatabaseConnectionProvider.Instance();

            // Assert
            Assert.IsInstanceOf<DatabaseConnectionProvider>(provider);
            Assert.IsInstanceOf<FakeDbConnectionProvider>(provider);
        }

        [Test]
        public void DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances()
        {
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DatabaseConnectionProvider>(new FakeDbConnectionProvider());

            // Act
            var provider1 = DatabaseConnectionProvider.Instance();
            var provider2 = DatabaseConnectionProvider.Instance();

            // Assert
            Assert.IsInstanceOf<DatabaseConnectionProvider>(provider1);
            Assert.IsInstanceOf<DatabaseConnectionProvider>(provider2);
            Assert.AreSame(provider1, provider2);
        }
    }
}
