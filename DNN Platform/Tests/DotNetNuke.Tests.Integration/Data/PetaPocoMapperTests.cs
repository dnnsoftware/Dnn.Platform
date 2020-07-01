// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System;

    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Data.Models;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
    using PetaPoco;

    [TestFixture]
    public class PetaPocoMapperTests
    {
        // ReSharper disable InconsistentNaming
        [Test]
        public void PetaPocoMapper_Constructor_Initialises_TablePrefix_Property()
        {
            // Arrange

            // Act
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

            // Assert
            Assert.AreEqual(Constants.TABLENAME_Prefix, Util.GetPrivateMember<PetaPocoMapper, string>(mapper, "_tablePrefix"));
        }

        [Test]
        public void PetaPocoMapper_GetTableInfo_Returns_TableInfo()
        {
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            // Assert
            Assert.IsInstanceOf<TableInfo>(ti);
        }

        [Test]
        public void PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName()
        {
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            // Assert
            Assert.AreEqual(Constants.TABLENAME_Dog, ti.TableName);
        }

        [Test]
        public void PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName()
        {
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            // Assert
            Assert.AreEqual(Constants.TABLENAME_Prefix + Constants.TABLENAME_Dog, ti.TableName);
        }

        [Test]
        public void PetaPocoMapper_Maps_TableName_To_Attribute()
        {
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));

            // Assert
            Assert.AreEqual(Constants.TABLENAME_Person, ti.TableName);
        }

        [Test]
        public void PetaPocoMapper_Adds_Prefix_To_Attribute()
        {
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));

            // Assert
            Assert.AreEqual(Constants.TABLENAME_Prefix + Constants.TABLENAME_Person, ti.TableName);
        }

        [Test]
        public void PetaPocoMapper_Sets_PrimaryKey_To_Attribute()
        {
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));

            // Assert
            Assert.AreEqual(Constants.TABLENAME_Person_Key, ti.PrimaryKey);
        }

        [Test]
        public void PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute()
        {
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            // Assert
            Assert.AreEqual(Constants.TABLENAME_Key, ti.PrimaryKey);
        }

        [Test]
        public void PetaPocoMapper_Sets_AutoIncrement_To_True()
        {
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            // Assert
            Assert.IsTrue(ti.AutoIncrement);
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo()
        {
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var dogType = typeof(Dog);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);

            // Act
            var ci = mapper.GetColumnInfo(dogProperty);

            // Assert
            Assert.IsInstanceOf<ColumnInfo>(ci);
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute()
        {
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var personType = typeof(Person);
            var personProperty = personType.GetProperty(Constants.COLUMNNAME_Name);

            // Act
            var ci = mapper.GetColumnInfo(personProperty);

            // Assert
            Assert.AreEqual(Constants.COLUMNNAME_PersonName, ci.ColumnName);
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute()
        {
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var dogType = typeof(Dog);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);

            // Act
            var ci = mapper.GetColumnInfo(dogProperty);

            // Assert
            Assert.AreEqual(Constants.COLUMNNAME_Name, ci.ColumnName);
        }

        // ReSharper restore InconsistentNaming
    }
}
