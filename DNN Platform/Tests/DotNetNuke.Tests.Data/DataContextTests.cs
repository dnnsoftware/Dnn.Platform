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

using System.Collections.Generic;
using System.Configuration;

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;
using PetaPoco;

namespace DotNetNuke.Tests.Data
{
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
                {"name", "SqlDataProvider"},
                {"type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke"},
                {"connectionStringName", "SiteSqlServer"},
                {"objectQualifier", ""},
                {"databaseOwner", "dbo."}
            });
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void DataContext_Instance_Method_Returns_PetaPocoDataContext()
        {
            //Arrange

            //Act
            var context = DataContext.Instance();

            //Assert
            Assert.IsInstanceOf<IDataContext>(context);
            Assert.IsInstanceOf<PetaPocoDataContext>(context);
        }

        [Test]
        public void DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()
        {
            //Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            //Act
            var context = (PetaPocoDataContext)DataContext.Instance();

            //Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Assert.AreEqual(connectionString, Util.GetPrivateMember<Database, string>(db, "_connectionString"));
        }

        [Test]
        [TestCase("PetaPoco")]
        [TestCase("Test")]
        public void DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(string name)
        {
            //Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;

            //Act
            var context = (PetaPocoDataContext)DataContext.Instance(name);

            //Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Assert.AreEqual(connectionString, Util.GetPrivateMember<Database, string>(db, "_connectionString"));
        }

        // ReSharper restore InconsistentNaming
    }
}
