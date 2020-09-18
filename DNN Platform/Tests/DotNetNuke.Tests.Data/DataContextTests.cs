// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System.Collections.Generic;
    using System.Configuration;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
    using PetaPoco;

    [TestFixture]
    public class DataContextTests
    {
        // ReSharper disable InconsistentNaming
        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider());
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>()
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
        }

        [Test]
        public void DataContext_Instance_Method_Returns_PetaPocoDataContext()
        {
            // Arrange

            // Act
            var context = DataContext.Instance();

            // Assert
            Assert.IsInstanceOf<IDataContext>(context);
            Assert.IsInstanceOf<PetaPocoDataContext>(context);
        }

        [Test]
        public void DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()
        {
            // Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            // Act
            var context = (PetaPocoDataContext)DataContext.Instance();

            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Assert.AreEqual(connectionString, Util.GetPrivateMember<Database, string>(db, "_connectionString"));
        }

        [Test]
        [TestCase("PetaPoco")]
        [TestCase("Test")]
        public void DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(string name)
        {
            // Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;

            // Act
            var context = (PetaPocoDataContext)DataContext.Instance(name);

            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Assert.AreEqual(connectionString, Util.GetPrivateMember<Database, string>(db, "_connectionString"));
        }

        // ReSharper restore InconsistentNaming
    }
}
