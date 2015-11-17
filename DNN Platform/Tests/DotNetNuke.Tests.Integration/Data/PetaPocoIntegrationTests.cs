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
using System.Data;
using System.Linq;

using DotNetNuke.Collections;
using DotNetNuke.ComponentModel;
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
    public class PetaPocoIntegrationTests
    {
        private Dictionary<Type, IMapper> _mappers;

        // ReSharper disable InconsistentNaming
        #region Setup/Teardown

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

            var dogMapper = new FluentMapper<Dog>("")
                    .TableName(Constants.PETAPOCO_DogTableName)
                    .PrimaryKey("ID")
                    .Property(d => d.ID, "ID")
                    .Property(d => d.Age, "Age")
                    .Property(d => d.Name, "Name");

            var catMapper = new FluentMapper<Cat>("")
                    .TableName(Constants.PETAPOCO_DogTableName)
                    .Property(d => d.Age, "Age")
                    .Property(d => d.Name, "Name");

            _mappers = new Dictionary<Type, IMapper> {{typeof (Dog), dogMapper}};
            _mappers = new Dictionary<Type, IMapper> { { typeof(Cat), catMapper } };
        }

        [TearDown]
        public void TearDown()
        {
            DataUtil.DeleteDatabase(Constants.PETAPOCO_DatabaseName);
        }

        #endregion

        private const string ConnectionStringName = "PetaPoco";

        [Test]
        public void PetaPoco_Add_Inserts_Item()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
                          {
                              Age = Constants.PETAPOCO_InsertDogAge,
                              Name = Constants.PETAPOCO_InsertDogName
                          };

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogRepository.Insert(dog);
            }

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount + 1, actualCount);
        }

        [Test]
        public void PetaPoco_Add_Inserts_Item_Using_FluentMapper()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
                            {
                                Age = Constants.PETAPOCO_InsertDogAge,
                                Name = Constants.PETAPOCO_InsertDogName
                            };

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName, String.Empty, _mappers))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogRepository.Insert(dog);
            }

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount + 1, actualCount);
        }

        [Test]
        public void PetaPoco_Delete_Deletes_Item()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
                          {
                              ID = Constants.PETAPOCO_DeleteDogId,
                              Age = Constants.PETAPOCO_DeleteDogAge,
                              Name = Constants.PETAPOCO_DeleteDogName
                          };

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogRepository.Delete(dog);
            }

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount - 1, actualCount);
        }

        [Test]
        public void PetaPoco_Delete_Deletes_Item_Using_FluentMapper()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
                            {
                                ID = Constants.PETAPOCO_DeleteDogId,
                                Age = Constants.PETAPOCO_DeleteDogAge,
                                Name = Constants.PETAPOCO_DeleteDogName
                            };

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName, String.Empty, _mappers))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogRepository.Delete(dog);
            }

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount - 1, actualCount);
        }

        [Test]
        public void PetaPoco_Delete_Overload_Deletes_Item()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogRepository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);
            }

            //Assert
            int actualCount = DataUtil.GetRecordCount(Constants.PETAPOCO_DatabaseName,
                                                      Constants.PETAPOCO_DogTableName);
            Assert.AreEqual(Constants.PETAPOCO_RecordCount - 1, actualCount);
        }

        [Test]
        [TestCase(1, "WHERE ID < @0", 2)]
        [TestCase(4, "WHERE Age <= @0", 5)]
        [TestCase(2, "WHERE Name LIKE @0", "B%")]
        public void PetaPoco_Find_Returns_Correct_Rows(int count, string sqlCondition, object arg)
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            IEnumerable<Dog> dogs;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogs = dogRepository.Find(sqlCondition, arg);
            }

            //Assert
            Assert.AreEqual(count, dogs.Count());
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void PetaPoco_Get_Returns_All_Items(int count)
        {
            //Arrange
            DataUtil.SetUpDatabase(count);

            IEnumerable<Dog> dogs;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogs = dogRepository.Get();
            }


            //Assert
            Assert.AreEqual(count, dogs.Count());
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Last, Constants.PAGE_RecordCount)]
        public void PetaPoco_GetAll_Overload_Returns_Page_Of_Items(int pageIndex, int pageSize)
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            IPagedList<Dog> dogs;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogs = dogRepository.GetPage(pageIndex, pageSize);
            }


            //Assert
            Assert.AreEqual(pageSize, dogs.PageSize);
        }

        [Test]
        public void PetaPoco_GetById_Returns_Single_Item()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            Dog dog;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dog = dogRepository.GetById(Constants.PETAPOCO_ValidDogId);
            }

            //Assert
            Assert.IsInstanceOf<Dog>(dog);
            Assert.AreEqual(Constants.PETAPOCO_ValidDogAge, dog.Age);
            Assert.AreEqual(Constants.PETAPOCO_ValidDogName, dog.Name);
        }

        [Test]
        public void PetaPoco_GetById_Returns_Single_Item_Using_FluentMapper()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            Dog dog;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName, String.Empty, _mappers))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dog = dogRepository.GetById(Constants.PETAPOCO_ValidDogId);
            }

            //Assert
            Assert.IsInstanceOf<Dog>(dog);
            Assert.AreEqual(Constants.PETAPOCO_ValidDogAge, dog.Age);
            Assert.AreEqual(Constants.PETAPOCO_ValidDogName, dog.Name);
        }

        [Test]
        public void PetaPoco_Update_Updates_Item()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
                          {
                              ID = Constants.PETAPOCO_UpdateDogId,
                              Age = Constants.PETAPOCO_UpdateDogAge,
                              Name = Constants.PETAPOCO_UpdateDogName
                          };

            //Act
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogRepository.Update(dog);
            }

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);

            Assert.AreEqual(Constants.PETAPOCO_RecordCount, table.Rows.Count);

            foreach (DataRow row in table.Rows)
            {
                if ((int) row["ID"] == Constants.PETAPOCO_UpdateDogId)
                {
                    Assert.AreEqual(row["Age"], Constants.PETAPOCO_UpdateDogAge);
                    Assert.AreEqual(row["Name"], Constants.PETAPOCO_UpdateDogName);
                }
            }
        }

        [Test]
        public void PetaPoco_Update_Updates_Item_Using_FluentMapper()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
                            {
                                ID = Constants.PETAPOCO_UpdateDogId,
                                Age = Constants.PETAPOCO_UpdateDogAge,
                                Name = Constants.PETAPOCO_UpdateDogName
                            };

            //Act
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName, String.Empty, _mappers))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogRepository.Update(dog);
            }

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);

            Assert.AreEqual(Constants.PETAPOCO_RecordCount, table.Rows.Count);

            foreach (DataRow row in table.Rows)
            {
                if ((int)row["ID"] == Constants.PETAPOCO_UpdateDogId)
                {
                    Assert.AreEqual(row["Age"], Constants.PETAPOCO_UpdateDogAge);
                    Assert.AreEqual(row["Name"], Constants.PETAPOCO_UpdateDogName);
                }
            }
        }
        [Test]
        public void PetaPoco_Update_Overload_Updates_Item()
        {
            //Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            //Act
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                //Act
                dogRepository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);
            }

            //Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);

            Assert.AreEqual(Constants.PETAPOCO_RecordCount, table.Rows.Count);

            foreach (DataRow row in table.Rows)
            {
                if ((int)row["ID"] == Constants.PETAPOCO_UpdateDogId)
                {
                    Assert.AreEqual(row["Age"], Constants.PETAPOCO_UpdateDogAge);
                    Assert.AreEqual(row["Name"], Constants.PETAPOCO_UpdateDogName);
                }
            }
        }

        // ReSharper restore InconsistentNaming
    }
}