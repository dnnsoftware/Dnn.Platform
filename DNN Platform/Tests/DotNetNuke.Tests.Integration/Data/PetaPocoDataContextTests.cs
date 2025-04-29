// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Data;

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
        Assert.That(Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database"), Is.InstanceOf<Database>());
    }

    [Test]
    public void PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()
    {
        // Arrange

        // Act
        var context = new PetaPocoDataContext(connectionStringName, tablePrefix);

        // Assert
        Assert.That(context.TablePrefix, Is.EqualTo(tablePrefix));
    }

    [Test]
    public void PetaPocoDataContext_Constructor_Initialises_Mapper_Property()
    {
        // Arrange

        // Act
        var context = new PetaPocoDataContext(connectionStringName);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoDataContext, IMapper>(context, "_mapper"), Is.InstanceOf<IMapper>());
            Assert.That(Util.GetPrivateMember<PetaPocoDataContext, PetaPocoMapper>(context, "_mapper"), Is.InstanceOf<PetaPocoMapper>());
        });
    }

    [Test]
    public void PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()
    {
        // Arrange

        // Act
        var context = new PetaPocoDataContext(connectionStringName);

        // Assert
        Assert.That(context.FluentMappers, Is.InstanceOf<Dictionary<Type, IMapper>>());
        Assert.That(context.FluentMappers, Is.Empty);
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

        Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
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
        Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth + 1));
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
        Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth - 1));
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
        Assert.That(Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"), Is.EqualTo(false));
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
        Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth - 1));
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
        Assert.That(Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"), Is.EqualTo(true));
    }

    [Test]
    public void PetaPocoDataContext_GetRepository_Returns_Repository()
    {
        // Arrange
        var context = new PetaPocoDataContext(connectionStringName);

        // Act
        var repo = context.GetRepository<Dog>();

        // Assert
        Assert.That(repo, Is.InstanceOf<IRepository<Dog>>());
        Assert.That(repo, Is.InstanceOf<PetaPocoRepository<Dog>>());
    }

    [Test]
    public void PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()
    {
        // Arrange
        var context = new PetaPocoDataContext(connectionStringName);

        // Act
        var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

        // Assert
        Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, Database>(repo, "_database"), Is.InstanceOf<Database>());
    }

    [Test]
    public void PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()
    {
        // Arrange
        var context = new PetaPocoDataContext(connectionStringName);

        // Act
        var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

        // Assert
        Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<IMapper>());
        Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<PetaPocoMapper>());
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
        Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<IMapper>());
        Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<FluentMapper<Dog>>());
    }

    // ReSharper restore InconsistentNaming
}
