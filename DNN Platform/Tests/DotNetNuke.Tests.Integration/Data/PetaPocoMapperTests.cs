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

using System;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Tests.Data.Models;
using DotNetNuke.Tests.Utilities;

using NUnit.Framework;

using PetaPoco;

namespace DotNetNuke.Tests.Data
{
    [TestFixture]
    public class PetaPocoMapperTests
    {
        // ReSharper disable InconsistentNaming

        #region Constructor Tests

        [Test]
        public void PetaPocoMapper_Constructor_Initialises_TablePrefix_Property()
        {
            //Arrange

            //Act
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Prefix, Util.GetPrivateMember<PetaPocoMapper, string>(mapper, "_tablePrefix"));
        }

        #endregion

        #region GetTableInfo Tests

        [Test]
        public void PetaPocoMapper_GetTableInfo_Returns_TableInfo()
        {
            //Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

            //Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            //Assert
            Assert.IsInstanceOf<TableInfo>(ti);
        }

        [Test]
        public void PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName()
        {
            //Arrange
            var mapper = new PetaPocoMapper(String.Empty);

            //Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Dog, ti.TableName);
        }

        [Test]
        public void PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName()
        {
            //Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

            //Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Prefix + Constants.TABLENAME_Dog, ti.TableName);
        }

        [Test]
        public void PetaPocoMapper_Maps_TableName_To_Attribute()
        {
            //Arrange
            var mapper = new PetaPocoMapper(String.Empty);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Person, ti.TableName);
        }

        [Test]
        public void PetaPocoMapper_Adds_Prefix_To_Attribute()
        {
            //Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Prefix + Constants.TABLENAME_Person, ti.TableName);
        }

        [Test]
        public void PetaPocoMapper_Sets_PrimaryKey_To_Attribute()
        {
            //Arrange
            var mapper = new PetaPocoMapper(String.Empty);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Person_Key, ti.PrimaryKey);
        }

        [Test]
        public void PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute()
        {
            //Arrange
            var mapper = new PetaPocoMapper(String.Empty);

            //Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Key, ti.PrimaryKey);
        }

        [Test]
        public void PetaPocoMapper_Sets_AutoIncrement_To_True()
        {
            //Arrange
            var mapper = new PetaPocoMapper(String.Empty);

            //Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            //Assert
            Assert.IsTrue(ti.AutoIncrement);
        }

        #endregion

        #region GetColumnInfo Tests

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo()
        {
            //Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var dogType = typeof (Dog);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);

            //Act
            var ci = mapper.GetColumnInfo(dogProperty);

            //Assert
            Assert.IsInstanceOf<ColumnInfo>(ci);
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute()
        {
            //Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var personType = typeof(Person);
            var personProperty = personType.GetProperty(Constants.COLUMNNAME_Name);

            //Act
            var ci = mapper.GetColumnInfo(personProperty);

            //Assert
            Assert.AreEqual(Constants.COLUMNNAME_PersonName, ci.ColumnName);
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute()
        {
            //Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var dogType = typeof(Dog);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);

            //Act
            var ci = mapper.GetColumnInfo(dogProperty);

            //Assert
            Assert.AreEqual(Constants.COLUMNNAME_Name, ci.ColumnName);
        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}
