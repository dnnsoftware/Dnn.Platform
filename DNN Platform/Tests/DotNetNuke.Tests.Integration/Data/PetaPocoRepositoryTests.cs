// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
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
            DataUtil.DeleteDatabase(Constants.PETAPOCO_DatabaseName);
        }

        [Test]
        public void PetaPocoRepository_Constructor_Throws_On_Null_Database()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => new PetaPocoRepository<Dog>(null, mockMapper.Object));
        }

        [Test]
        public void PetaPocoRepository_Constructor_Registers_Mapper()
        {
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var db = new Database(connectionStringName);

            // Act
#pragma warning disable 168
            var repo = new PetaPocoRepository<Dog>(db, mockMapper.Object);
#pragma warning restore 168

            // Assert
            Assert.AreSame(mockMapper.Object, Mappers.GetMapper(typeof(Dog), mockMapper.Object));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void PetaPocoRepository_Get_Returns_All_Rows(int count)
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(count);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            IEnumerable<Dog> dogs = repository.Get();

            // Assert
            Assert.AreEqual(count, dogs.Count());
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_List_Of_Models()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dogs = repository.Get().ToList();

            // Assert
            for (int i = 0; i < dogs.Count(); i++)
            {
                Assert.IsInstanceOf<Dog>(dogs[i]);
            }
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_Models_With_Correct_Properties()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dogs = repository.Get();

            // Assert
            var dog = dogs.First();
            Assert.AreEqual(this._dogAges[0], dog.Age.ToString());
            Assert.AreEqual(this._dogNames[0], dog.Name);
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dogs = repository.Get();

            // Assert
            var dog = dogs.First();
            Assert.AreEqual(this._dogAges[0], dog.Age.ToString());
            Assert.AreEqual(this._dogNames[0], dog.Name);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

            // Assert
            Assert.IsInstanceOf<Dog>(dog);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Null_If_InValid_Id()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dog = repository.GetById(Constants.PETAPOCO_InvalidDogId);

            // Assert
            Assert.IsNull(dog);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dog = repository.GetById(Constants.PETAPOCO_InvalidDogId);

            // Assert
            Assert.IsNull(dog);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

            // Assert
            Assert.AreEqual(Constants.PETAPOCO_ValidDogAge, dog.Age);
            Assert.AreEqual(Constants.PETAPOCO_ValidDogName, dog.Name);
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

            // Assert
            Assert.AreEqual(Constants.PETAPOCO_ValidDogAge, dog.Age);
            Assert.AreEqual(Constants.PETAPOCO_ValidDogName, dog.Name);
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
            repository.Insert(dog);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount + 1, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
            repository.Insert(dog);

            // Assert
            int newId = DataUtil.GetLastAddedRecordID(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName, Constants.TABLENAME_Key);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount + 1, newId);
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
            repository.Insert(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            DataRow row = table.Rows[table.Rows.Count - 1];

            Assert.AreEqual(Constants.PETAPOCO_InsertDogAge, row["Age"]);
            Assert.AreEqual(Constants.PETAPOCO_InsertDogName, row["Name"]);
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
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
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
            repository.Insert(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            DataRow row = table.Rows[table.Rows.Count - 1];

            Assert.AreEqual(Constants.PETAPOCO_InsertDogAge, row["Age"]);
            Assert.AreEqual(Constants.PETAPOCO_InsertDogName, row["Name"]);
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount - 1, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                Assert.IsFalse((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId);
            }
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
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
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                Assert.IsFalse((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId);
            }
        }

        [Test]
        public void PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_InvalidDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);

            // Assert
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
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
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);

            // Assert
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount - 1, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
                Assert.IsFalse((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId);
            }
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_InvalidDogId);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        [Test]
        [TestCase(1, "WHERE ID < @0", 2)]
        [TestCase(4, "WHERE Age <= @0", 5)]
        [TestCase(2, "WHERE Name LIKE @0", "B%")]
        public void PetaPocoRepository_Find_Returns_Correct_Rows(int count, string sqlCondition, object arg)
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            IEnumerable<Dog> dogs = repository.Find(sqlCondition, arg);

            // Assert
            Assert.AreEqual(count, dogs.Count());
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Last, Constants.PAGE_RecordCount)]
        public void PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(int pageIndex, int pageSize)
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dogs = repository.GetPage(pageIndex, pageSize);

            // Assert
            Assert.AreEqual(pageSize, dogs.PageSize);
        }

        [Test]
        public void PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dogs = repository.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            for (int i = 0; i < dogs.Count(); i++)
            {
                Assert.IsInstanceOf<Dog>(dogs[i]);
            }
        }

        [Test]
        public void PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dogs = repository.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

            // Assert
            var dog = dogs.First();
            Assert.AreEqual(this._dogAges[0], dog.Age.ToString());
            Assert.AreEqual(this._dogNames[0], dog.Name);
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount, 1)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount, 6)]
        [TestCase(2, 4, 9)]
        public void PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(int pageIndex, int pageSize, int firstId)
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            var dogs = repository.GetPage(pageIndex, pageSize);

            // Assert
            var dog = dogs.First();
            Assert.AreEqual(firstId, dog.ID);
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_In_DataBase()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
            repository.Update(dog);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_With_Correct_ID()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
            repository.Update(dog);

            // Assert
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
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
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
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
            repository.Update(dog);

            // Assert
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
        public void PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            repository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount, actualCount);
        }

        [Test]
        public void PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID()
        {
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

            // Act
            repository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);

            // Assert
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

        private Database CreatePecaPocoDatabase()
        {
            var db = new Database(connectionStringName);

            return db;
        }

        // ReSharper restore InconsistentNaming
    }
}
