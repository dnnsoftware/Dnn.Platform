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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Tests.Data.Models;
using DotNetNuke.Tests.Utilities;

using Moq;

using NUnit.Framework;

using PetaPoco;

namespace DotNetNuke.Tests.Data
{
    [TestFixture]
    public class PetaPocoRepositoryTests
    {
        // ReSharper disable InconsistentNaming

        private const string connectionStringName = "PetaPoco";
        private readonly string[] _dogAges = Constants.PETAPOCO_DogAges.Split(',');
        private readonly string[] _dogNames = Constants.PETAPOCO_DogNames.Split(',');

        [SetUp]
        public void SetUp()
        {
            Mappers.Revoke(typeof(Dog));
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
            DataUtil.DeleteDatabase(Constants.PETAPOCO_DatabaseName);
        }

        #region Constructor Tests

        [Test]
        public void PetaPocoRepository_Constructor_Throws_On_Null_Database()
        {
            //Arrange
            var mockMapper = new Mock<IMapper>();

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => new PetaPocoRepository<Dog>(null, mockMapper.Object));
        }

        [Test]
        public void PetaPocoRepository_Constructor_Registers_Mapper()
        {
            //Arrange
            var mockMapper = new Mock<IMapper>();
            var db = new Database(connectionStringName);

            //Act
#pragma warning disable 168
            var repo = new PetaPocoRepository<Dog>(db, mockMapper.Object);
#pragma warning restore 168

            //Assert
            Assert.AreSame(mockMapper.Object, Mappers.GetMapper(typeof(Dog)));
        }

        #endregion

        #region Get Tests

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void PetaPocoRepository_Get_Returns_All_Rows(int count)
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(count);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            IEnumerable<Dog> dogs = repository.Get();

            //Assert
            Assert.AreEqual(count, dogs.Count());
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_List_Of_Models()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dogs = repository.Get().ToList();

            //Assert
            for (int i = 0; i < dogs.Count(); i++)
            {
                Assert.IsInstanceOf<Dog>(dogs[i]);
            }
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_Models_With_Correct_Properties()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dogs = repository.Get();

            //Assert
            var dog = dogs.First();
            Assert.AreEqual(_dogAges[0], dog.Age.ToString());
            Assert.AreEqual(_dogNames[0], dog.Name);
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(String.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dogs = repository.Get();

            //Assert
            var dog = dogs.First();
            Assert.AreEqual(_dogAges[0], dog.Age.ToString());
            Assert.AreEqual(_dogNames[0], dog.Name);
        }

        #endregion

        #region GetById Tests

        [Test]
        public void PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

            //Assert
            Assert.IsInstanceOf<Dog>(dog);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Null_If_InValid_Id()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dog = repository.GetById(Constants.PETAPOCO_InvalidDogId);

            //Assert
            Assert.IsNull(dog);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(String.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dog = repository.GetById(Constants.PETAPOCO_InvalidDogId);

            //Assert
            Assert.IsNull(dog);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

            //Assert
            Assert.AreEqual(Constants.PETAPOCO_ValidDogAge, dog.Age);
            Assert.AreEqual(Constants.PETAPOCO_ValidDogName, dog.Name);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(String.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

            //Assert
            Assert.AreEqual(Constants.PETAPOCO_ValidDogAge, dog.Age);
            Assert.AreEqual(Constants.PETAPOCO_ValidDogName, dog.Name);
        }

        #endregion

        #region Add Tests

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
                            {
                                Age = Constants.PETAPOCO_InsertDogAge,
                                Name = Constants.PETAPOCO_InsertDogName
                            };

            //Act
            repository.Insert(dog);

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount + 1, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
                            {
                                Age = Constants.PETAPOCO_InsertDogAge,
                                Name = Constants.PETAPOCO_InsertDogName
                            };

            //Act
            repository.Insert(dog);

            //Assert
            int newId = DataUtil.GetLastAddedRecordID(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName, Constants.TABLENAME_Key);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount + 1, newId);
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
                            {
                                Age = Constants.PETAPOCO_InsertDogAge,
                                Name = Constants.PETAPOCO_InsertDogName
                            };

            //Act
            repository.Insert(dog);

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            DataRow row = table.Rows[table.Rows.Count - 1];

            Assert.AreEqual(Constants.PETAPOCO_InsertDogAge, row["Age"]);
            Assert.AreEqual(Constants.PETAPOCO_InsertDogName, row["Name"]);
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(String.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName
            };

            //Act
            repository.Insert(dog);

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            DataRow row = table.Rows[table.Rows.Count - 1];

            Assert.AreEqual(Constants.PETAPOCO_InsertDogAge, row["Age"]);
            Assert.AreEqual(Constants.PETAPOCO_InsertDogName, row["Name"]);
        }

        #endregion

        #region Delete Tests

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
                            {
                                ID = Constants.PETAPOCO_DeleteDogId,
                                Age = Constants.PETAPOCO_DeleteDogAge,
                                Name = Constants.PETAPOCO_DeleteDogName
                            };

            //Act
            repository.Delete(dog);

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount - 1, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
                            {
                                ID = Constants.PETAPOCO_DeleteDogId,
                                Age = Constants.PETAPOCO_DeleteDogAge,
                                Name = Constants.PETAPOCO_DeleteDogName
                            };

            //Act
            repository.Delete(dog);

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                Assert.IsFalse((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId);
            }
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(String.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName
            };

            //Act
            repository.Delete(dog);

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                Assert.IsFalse((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId);
            }
        }

        [Test]
        public void PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
                            {
                                ID = Constants.PETAPOCO_InvalidDogId,
                                Age = Constants.PETAPOCO_DeleteDogAge,
                                Name = Constants.PETAPOCO_DeleteDogName
                            };

            //Act
            repository.Delete(dog);

            //Assert
            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(String.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_InvalidDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName
            };

            //Act
            repository.Delete(dog);

            //Assert
            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        #endregion

        #region Delete(sql, args) Tests

        [Test]
        public void PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount - 1, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                Assert.IsFalse((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId);
            }
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_InvalidDogId);

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        #endregion

        #region Find Tests

        [Test]
        [TestCase(1, "WHERE ID < @0", 2)]
        [TestCase(4, "WHERE Age <= @0", 5)]
        [TestCase(2, "WHERE Name LIKE @0", "B%")]
        public void PetaPocoRepository_Find_Returns_Correct_Rows(int count, string sqlCondition, object arg)
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            IEnumerable<Dog> dogs = repository.Find(sqlCondition, arg);

            //Assert
            Assert.AreEqual(count, dogs.Count());
        }

        #endregion

        #region GetPage Tests

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Last, Constants.PAGE_RecordCount)]
        public void PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(int pageIndex, int pageSize)
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dogs = repository.GetPage(pageIndex, pageSize);

            //Assert
            Assert.AreEqual(pageSize, dogs.PageSize);
        }

        [Test]
        public void PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dogs = repository.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            //Assert
            for (int i = 0; i < dogs.Count(); i++)
            {
                Assert.IsInstanceOf<Dog>(dogs[i]);
            }
        }

        [Test]
        public void PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dogs = repository.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            //Assert
            var dog = dogs.First();
            Assert.AreEqual(_dogAges[0], dog.Age.ToString());
            Assert.AreEqual(_dogNames[0], dog.Name);
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount, 1)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount, 6)]
        [TestCase(2, 4, 9)]
        public void PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(int pageIndex, int pageSize, int firstId)
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            var dogs = repository.GetPage(pageIndex, pageSize);

            //Assert
            var dog = dogs.First();
            Assert.AreEqual(firstId, dog.ID);
        }

        #endregion

        #region Update Tests

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_In_DataBase()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
                            {
                                ID = Constants.PETAPOCO_UpdateDogId,
                                Age = Constants.PETAPOCO_UpdateDogAge,
                                Name = Constants.PETAPOCO_UpdateDogName
                            };

            //Act
            repository.Update(dog);

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_With_Correct_ID()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
                            {
                                ID = Constants.PETAPOCO_UpdateDogId,
                                Age = Constants.PETAPOCO_UpdateDogAge,
                                Name = Constants.PETAPOCO_UpdateDogName
                            };

            //Act
            repository.Update(dog);

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    Assert.AreEqual(Constants.PETAPOCO_UpdateDogAge, row["Age"]);
                    Assert.AreEqual(Constants.PETAPOCO_UpdateDogName, row["Name"]);
                }
            }
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(String.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName
            };

            //Act
            repository.Update(dog);

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    Assert.AreEqual(Constants.PETAPOCO_UpdateDogAge, row["Age"]);
                    Assert.AreEqual(Constants.PETAPOCO_UpdateDogName, row["Name"]);
                }
            }
        }

        #endregion

        #region Update(sql, args) Tests

        [Test]
        public void PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            repository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID()
        {
            //Arrange
            var db = CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(String.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            //Act
            repository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    Assert.AreEqual(Constants.PETAPOCO_UpdateDogAge, row["Age"]);
                    Assert.AreEqual(Constants.PETAPOCO_UpdateDogName, row["Name"]);
                }
            }
        }

        #endregion

        private Database CreatePecaPocoDatabase()
        {
            var db = new Database(connectionStringName);

            return db;
        }

        // ReSharper restore InconsistentNaming
    }
}
