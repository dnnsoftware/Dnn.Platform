
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Tests.Data.Models;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;
using PetaPoco;

namespace DotNetNuke.Tests.Data
{
    [TestFixture]
    public class FluentMapperTests
    {

        [Test]
        public void FluentMapper_Constructor_Initialises_TablePrefix_Property()
        {
            //Arrange

            //Act
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Prefix, Util.GetPrivateMember<FluentMapper<Dog>, string>(mapper, "_tablePrefix"));
        }

        [Test]
        public void FluentMapper_GetTableInfo_Returns_TableInfo()
        {
            //Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);

            //Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            //Assert
            Assert.IsInstanceOf<TableInfo>(ti);
        }

        [Test]
        public void FluentMapper_Maps_TableName_To_Value_Provided()
        {
            //Arrange
            var mapper = new FluentMapper<Dog>(String.Empty);
            mapper.TableName(Constants.TABLENAME_Dog);

            //Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Dog, ti.TableName);
        }

        [Test]
        public void FluentMapper_Adds_TablePrefix_To_Value_Provided()
        {
            //Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
            mapper.TableName(Constants.TABLENAME_Dog);

            //Act
            var ti = mapper.GetTableInfo(typeof(Dog));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Prefix + Constants.TABLENAME_Dog, ti.TableName);
        }

        [Test]
        public void FluentMapper_Does_Not_Map_TableName_To_Attribute()
        {
            //Arrange
            var mapper = new FluentMapper<Person>(String.Empty);
            mapper.TableName(Constants.TABLENAME_Dog);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.AreNotEqual(Constants.TABLENAME_Person, ti.TableName);
            Assert.AreEqual(Constants.TABLENAME_Dog, ti.TableName);
        }

        [Test]
        public void FluentMapper_Sets_TableName_To_Null_If_Not_Provided()
        {
            //Arrange
            var mapper = new FluentMapper<Person>(String.Empty);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.IsNull(ti.TableName);
        }

        [Test]
        public void FluentMapper_Sets_PrimaryKey_To_Provided_Value()
        {
            //Arrange
            var mapper = new FluentMapper<Person>(String.Empty);
            mapper.PrimaryKey(Constants.TABLENAME_Key);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.AreEqual(Constants.TABLENAME_Key, ti.PrimaryKey);
        }

        [Test]
        public void FluentMapper_Does_Not_Set_PrimaryKey_To_Attribute()
        {
            //Arrange
            var mapper = new FluentMapper<Person>(String.Empty);
            mapper.PrimaryKey(Constants.TABLENAME_Key);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.AreNotEqual(Constants.TABLENAME_Person_Key, ti.PrimaryKey);
        }

        [Test]
        public void FluentMapper_Sets_PrimaryKey_To_Null_If_Not_Provided()
        {
            //Arrange
            var mapper = new FluentMapper<Person>(String.Empty);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.IsNull(ti.PrimaryKey);
        }

        [Test]
        public void FluentMapper_Sets_AutoIncrement_To_False()
        {
            //Arrange
            var mapper = new FluentMapper<Person>(String.Empty);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.IsFalse(ti.AutoIncrement);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FluentMapper_Sets_AutoIncrement_To_Value_Provided(bool autoIncrement)
        {
            //Arrange
            var mapper = new FluentMapper<Person>(String.Empty);
            mapper.PrimaryKey("ID", autoIncrement);

            //Act
            var ti = mapper.GetTableInfo(typeof(Person));

            //Assert
            Assert.AreEqual(autoIncrement, ti.AutoIncrement);
        }

        [Test]
        public void FluentMapper_GetColumnInfo_Returns_Null_If_Not_Provided()
        {
            //Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);

            var dogType = typeof(Dog);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);

            //Act
            var ci = mapper.GetColumnInfo(dogProperty);

            //Assert
            Assert.IsNull(ci);
        }

        [Test]
        public void FluentMapper_GetColumnInfo_Returns_ColumnInfo()
        {
            //Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
            mapper.Property(d => d.Name, Constants.COLUMNNAME_Name);

            var dogType = typeof(Dog);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);

            //Act
            var ci = mapper.GetColumnInfo(dogProperty);

            //Assert
            Assert.IsInstanceOf<ColumnInfo>(ci);
        }

        [Test]
        public void FluentMapper_GetColumnInfo_Maps_ColumnName_To_Value_Provided()
        {
            //Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
            mapper.Property(d => d.Name, Constants.COLUMNNAME_Name);

            var personType = typeof(Dog);
            var personProperty = personType.GetProperty(Constants.COLUMNNAME_Name);

            //Act
            var ci = mapper.GetColumnInfo(personProperty);

            //Assert
            Assert.AreEqual(Constants.COLUMNNAME_Name, ci.ColumnName);
        }

        [Test]
        public void FluentMapper_GetColumnInfo_Does_Not_Map_ColumnName_To_Attribute()
        {
            //Arrange
            var mapper = new FluentMapper<Person>(Constants.TABLENAME_Prefix);
            mapper.Property(d => d.Name, Constants.COLUMNNAME_Name);

            var personType = typeof(Person);
            var personProperty = personType.GetProperty(Constants.COLUMNNAME_Name);

            //Act
            var ci = mapper.GetColumnInfo(personProperty);

            //Assert
            Assert.AreNotEqual(Constants.COLUMNNAME_PersonName, ci.ColumnName);
        }
    }
}
