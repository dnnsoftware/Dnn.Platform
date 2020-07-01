// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Data.Models;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
    using PetaPoco;

    [TestFixture]
    public class PetaPocoDataContextTests
    {
        private const string connectionStringName = "PetaPoco";
        private const string tablePrefix = "dnn_";

        // ReSharper disable InconsistentNaming
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void PetaPocoDataContext_Constructors_Throw_On_Null_ConnectionString()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(null));
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(null, tablePrefix));
        }

        [Test]
        public void PetaPocoDataContext_Constructors_Throw_On_Empty_ConnectionString()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(string.Empty));
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(string.Empty, tablePrefix));
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Database_Property()
        {
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);

            // Assert
            Assert.IsInstanceOf<Database>(Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database"));
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()
        {
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName, tablePrefix);

            // Assert
            Assert.AreEqual(tablePrefix, context.TablePrefix);
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Mapper_Property()
        {
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);

            // Assert
            Assert.IsInstanceOf<IMapper>(Util.GetPrivateMember<PetaPocoDataContext, IMapper>(context, "_mapper"));
            Assert.IsInstanceOf<PetaPocoMapper>(Util.GetPrivateMember<PetaPocoDataContext, PetaPocoMapper>(context, "_mapper"));
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()
        {
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);

            // Assert
            Assert.IsInstanceOf<Dictionary<Type, IMapper>>(context.FluentMappers);
            Assert.AreEqual(0, context.FluentMappers.Count);
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection()
        {
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);

            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            Assert.AreEqual(connectionString, Util.GetPrivateMember<Database, string>(db, "_connectionString"));
        }

        [Test]
        public void PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count()
        {
            // Arrange
            const int transactionDepth = 1;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.BeginTransaction();

            // Assert
            Assert.AreEqual(transactionDepth + 1, Util.GetPrivateMember<Database, int>(db, "_transactionDepth"));
        }

        [Test]
        public void PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count()
        {
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.Commit();

            // Assert
            Assert.AreEqual(transactionDepth - 1, Util.GetPrivateMember<Database, int>(db, "_transactionDepth"));
        }

        [Test]
        public void PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False()
        {
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.Commit();

            // Assert
            Assert.AreEqual(false, Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"));
        }

        [Test]
        public void PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count()
        {
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.RollbackTransaction();

            // Assert
            Assert.AreEqual(transactionDepth - 1, Util.GetPrivateMember<Database, int>(db, "_transactionDepth"));
        }

        [Test]
        public void PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True()
        {
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.RollbackTransaction();

            // Assert
            Assert.AreEqual(true, Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"));
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Returns_Repository()
        {
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);

            // Act
            var repo = context.GetRepository<Dog>();

            // Assert
            Assert.IsInstanceOf<IRepository<Dog>>(repo);
            Assert.IsInstanceOf<PetaPocoRepository<Dog>>(repo);
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()
        {
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);

            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

            // Assert
            Assert.IsInstanceOf<Database>(Util.GetPrivateMember<PetaPocoRepository<Dog>, Database>(repo, "_database"));
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()
        {
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);

            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

            // Assert
            Assert.IsInstanceOf<IMapper>(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"));
            Assert.IsInstanceOf<PetaPocoMapper>(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"));
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined()
        {
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);
            context.AddFluentMapper<Dog>();

            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

            // Assert
            Assert.IsInstanceOf<IMapper>(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"));
            Assert.IsInstanceOf<FluentMapper<Dog>>(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"));
        }

        // ReSharper restore InconsistentNaming
    }
}
