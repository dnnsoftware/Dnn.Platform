#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

#region Usings

using System;
using System.Collections.Generic;
using System.Configuration;
using DotNetNuke.Data;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Tests.Data.Models;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

using PetaPoco;

#endregion

namespace DotNetNuke.Tests.Data
{
    [TestFixture]
    public class PetaPocoDataContextTests
    {
        // ReSharper disable InconsistentNaming
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        private const string connectionStringName = "PetaPoco";
        private const string tablePrefix = "dnn_";

        #region Constructor Tests

        [Test]
        public void PetaPocoDataContext_Constructors_Throw_On_Null_ConnectionString()
        {
            //Arrange

            //Act, Assert
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(null));
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(null, tablePrefix));
        }

        [Test]
        public void PetaPocoDataContext_Constructors_Throw_On_Empty_ConnectionString()
        {
            //Arrange

            //Act, Assert
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(String.Empty));
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(String.Empty, tablePrefix));
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Database_Property()
        {
            //Arrange

            //Act
            var context = new PetaPocoDataContext(connectionStringName);

            //Assert
            Assert.IsInstanceOf<Database>(Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database"));
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()
        {
            //Arrange

            //Act
            var context = new PetaPocoDataContext(connectionStringName, tablePrefix);

            //Assert
            Assert.AreEqual(tablePrefix, context.TablePrefix);
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Mapper_Property()
        {
            //Arrange

            //Act
            var context = new PetaPocoDataContext(connectionStringName);

            //Assert
            Assert.IsInstanceOf<IMapper>(Util.GetPrivateMember<PetaPocoDataContext, IMapper>(context, "_mapper"));
            Assert.IsInstanceOf<PetaPocoMapper>(Util.GetPrivateMember<PetaPocoDataContext, PetaPocoMapper>(context, "_mapper"));
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()
        {
            //Arrange

            //Act
            var context = new PetaPocoDataContext(connectionStringName);

            //Assert
            Assert.IsInstanceOf<Dictionary<Type, IMapper>>(context.FluentMappers);
            Assert.AreEqual(0,context.FluentMappers.Count);
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection()
        {
            //Arrange

            //Act
            var context = new PetaPocoDataContext(connectionStringName);

            //Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            Assert.AreEqual(connectionString, Util.GetPrivateMember<Database, string>(db, "_connectionString"));
        }

        #endregion

        #region BeginTransaction Tests

        [Test]
        public void PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count()
        {
            //Arrange
            const int transactionDepth = 1;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            //Act
            context.BeginTransaction();

            //Assert
            Assert.AreEqual(transactionDepth + 1, Util.GetPrivateMember<Database, int>(db, "_transactionDepth"));
        }

        #endregion

        #region Commit Tests

        [Test]
        public void PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count()
        {
            //Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            //Act
            context.Commit();

            //Assert
            Assert.AreEqual(transactionDepth - 1, Util.GetPrivateMember<Database, int>(db, "_transactionDepth"));
        }

        [Test]
        public void PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False()
        {
            //Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            //Act
            context.Commit();

            //Assert
            Assert.AreEqual(false, Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"));
        }

        #endregion

        #region RollbackTransaction Tests

        [Test]
        public void PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count()
        {
            //Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            //Act
            context.RollbackTransaction();

            //Assert
            Assert.AreEqual(transactionDepth - 1, Util.GetPrivateMember<Database, int>(db, "_transactionDepth"));
        }

        [Test]
        public void PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True()
        {
            //Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            //Act
            context.RollbackTransaction();

            //Assert
            Assert.AreEqual(true, Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"));
        }

        #endregion

        #region GetRepository Tests

        [Test]
        public void PetaPocoDataContext_GetRepository_Returns_Repository()
        {
            //Arrange
            var context = new PetaPocoDataContext(connectionStringName);

            //Act
            var repo = context.GetRepository<Dog>();

            //Assert
            Assert.IsInstanceOf<IRepository<Dog>>(repo);
            Assert.IsInstanceOf<PetaPocoRepository<Dog>>(repo);
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()
        {
            //Arrange
            var context = new PetaPocoDataContext(connectionStringName);

            //Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

            //Assert
            Assert.IsInstanceOf<Database>(Util.GetPrivateMember<PetaPocoRepository<Dog>, Database>(repo, "_database"));
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()
        {
            //Arrange
            var context = new PetaPocoDataContext(connectionStringName);

            //Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

            //Assert
            Assert.IsInstanceOf<IMapper>(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"));
            Assert.IsInstanceOf<PetaPocoMapper>(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"));
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined()
        {
            //Arrange
            var context = new PetaPocoDataContext(connectionStringName);
            context.AddFluentMapper<Dog>();

            //Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

            //Assert
            Assert.IsInstanceOf<IMapper>(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"));
            Assert.IsInstanceOf<FluentMapper<Dog>>(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"));
        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}