// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System.Collections.Generic;
    using System.Configuration;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Utilities;

    using Moq;

    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using PetaPoco;

    [TestFixture]
    public class DataContextTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IApplicationStatusInfo>()));
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider(Mock.Of<IApplicationStatusInfo>()));
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>
            {
                { "name", "SqlDataProvider" },
                { "type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke" },
                { "connectionStringName", "SiteSqlServer" },
                { "objectQualifier", string.Empty },
                { "databaseOwner", "dbo." },
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        public void DataContext_Instance_Method_Returns_PetaPocoDataContext()
        {
            // Arrange

            // Act
            var context = DataContext.Instance(Mock.Of<IHostSettings>());

            // Assert
            Assert.That(context, Is.InstanceOf<IDataContext>());
            Assert.That(context, Is.InstanceOf<PetaPocoDataContext>());
        }

        [Test]
        public void DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()
        {
            // Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            // Act
            var context = (PetaPocoDataContext)DataContext.Instance(Mock.Of<IHostSettings>());

            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "database");
            Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
        }

        [Test]
        [TestCase("PetaPoco")]
        [TestCase("Test")]
        public void DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(string name)
        {
            // Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;

            // Act
            var context = (PetaPocoDataContext)DataContext.Instance(Mock.Of<IHostSettings>(), name);

            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "database");
            Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
        }
    }
}
