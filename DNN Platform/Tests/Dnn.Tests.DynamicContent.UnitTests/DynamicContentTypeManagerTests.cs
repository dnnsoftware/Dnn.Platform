// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class DynamicContentTypeManagerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<DynamicContentType>> _mockContentTypeRepository;
        private Mock<IFieldDefinitionManager> _mockFieldDefinitionController;
        private Mock<IContentTemplateManager> _mockContentTemplateController;

        private Mock<CachingProvider> _mockCache;
        private string _contentTypeCacheKey;

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);

            _contentTypeCacheKey = CachingProvider.GetCacheKey(DataCache.ContentTypesCacheKey);

            _mockDataContext = new Mock<IDataContext>();

            _mockContentTypeRepository = new Mock<IRepository<DynamicContentType>>();
            _mockDataContext.Setup(dc => dc.GetRepository<DynamicContentType>()).Returns(_mockContentTypeRepository.Object);

            _mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            _mockFieldDefinitionController.Setup(vr => vr.GetFieldDefinitions(It.IsAny<int>()))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionController.Object);

            _mockContentTemplateController = new Mock<IContentTemplateManager>();
            _mockContentTemplateController.Setup(vr => vr.GetContentTemplates(It.IsAny<int>(), false))
                .Returns(new List<ContentTemplate>().AsQueryable());
            ContentTemplateManager.SetTestableInstance(_mockContentTemplateController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            UserController.ClearInstance();
        }

        [Test]
        public void Constructor_Throws_On_Null_DataContext()
        {
            IDataContext dataContent = null;

            //Arrange, Act, Arrange
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => new DynamicContentTypeManager(dataContent));
        }

        [Test]
        public void AddContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.AddContentType(null));
        }

        [Test]
        public void AddContentType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.AddContentType(new DynamicContentType()));
        }

        [Test]
        public void AddContentType_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType { Name = Constants.CONTENTTYPE_ValidContentType };

            //Act
            // ReSharper disable once UnusedVariable
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(rep => rep.Insert(contentType));
        }

        [Test]
        public void AddContentType_Returns_ValidId_On_Valid_ContentType()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType { Name = Constants.CONTENTTYPE_ValidContentType };

            //Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentTypeId);
        }

        [Test]
        public void AddContentType_Sets_ValidId_On_Valid_ContentType()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType { Name = Constants.CONTENTTYPE_ValidContentType };

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentType.ContentTypeId);
        }

        [Test]
        public void AddContentType_Adds_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType { Name = Constants.CONTENTTYPE_ValidContentType };

            var fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition());
            }

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            _mockFieldDefinitionController.Verify(fd => fd.AddFieldDefinition(It.IsAny<FieldDefinition>()), Times.Exactly(fieldDefinitionCount));
        }

        [Test]
        public void AddContentType_Sets_ContentTypeId_Property_Of_New_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_AddContentTypeId;
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = contentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType { Name = Constants.CONTENTTYPE_ValidContentType };

            var fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition());
            }

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreEqual(contentTypeId, field.ContentTypeId);
            }
        }

        [Test]
        public void AddContentType_Adds_ContentTemplates_On_Valid_ContentType()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType { Name = Constants.CONTENTTYPE_ValidContentType };

            var contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate());
            }

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            _mockContentTemplateController.Verify(ct => ct.AddContentTemplate(It.IsAny<ContentTemplate>()), Times.Exactly(contentTemplateCount));
        }

        [Test]
        public void AddContentType_Sets_ContentTypeId_Property_Of_New_ContentTemplates_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_AddContentTypeId;
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = contentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType { Name = Constants.CONTENTTYPE_ValidContentType };

            var contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate());
            }

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            foreach (var template in contentType.Templates)
            {
                Assert.AreEqual(contentTypeId, template.ContentTypeId);
            }
        }

        [Test]
        public void AddContentType_Sets_CreatedByUserId_Property_On_Valid_ContentType()
        {
            //Arrange
            int userId = Constants.USER_ValidId;
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetCurrentUserInfo()).Returns(new UserInfo { UserID = userId });
            UserController.SetTestableInstance(mockUserController.Object);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType { Name = Constants.CONTENTTYPE_ValidContentType };

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(userId, contentType.CreatedByUserId);
        }


        [Test]
        public void DeleteContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
        }

        [Test]
        public void DeleteContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                        {
                                            Name = Constants.CONTENTTYPE_ValidContentType,
                                            ContentTypeId = Null.NullInteger
                                        };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.DeleteContentType(contentType));
        }

        [Test]
        public void DeleteContentType_Calls_Repository_Delete_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                        {
                                            Name = Constants.CONTENTTYPE_ValidContentType,
                                            ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                        };

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Delete(contentType));
        }

        [Test]
        public void DeleteContentType_Deletes_FieldDefinitions_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                    {
                                        Name = Constants.CONTENTTYPE_ValidContentType,
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                    };

            var fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition());
            }

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockFieldDefinitionController.Verify(f => f.DeleteFieldDefinition(It.IsAny<FieldDefinition>()), Times.Exactly(fieldDefinitionCount));

        }

        [Test]
        public void DeleteContentType_Deletes_Templates_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                    {
                                        Name = Constants.CONTENTTYPE_ValidContentType,
                                        ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                    };

            var contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate());
            }

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockContentTemplateController.Verify(ct => ct.DeleteContentTemplate(It.IsAny<ContentTemplate>()), Times.Exactly(contentTemplateCount));
        }

        [Test]
        public void GetContentTypes_Calls_Repository_Get_With_PortalId()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var hostPortalId = -1;
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTypes = contentTypeController.GetContentTypes(portalId, true);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Get(hostPortalId));
        }

        [Test]
        public void GetContentTypes_Calls_Repository_Get_With_HostPortalId_If_IsSystem()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Get(Constants.PORTAL_ValidPortalId));
        }

        [Test]
        public void GetContentTypes_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(new List<DynamicContentType>());
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.IsNotNull(contentTypes);
            Assert.AreEqual(0, contentTypes.Count());
        }

        [Test]
        public void GetContentTypes_Returns_List_Of_ContentTypes()
        {
            //Arrange
            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(CreateValidContentTypes(Constants.CONTENTTYPE_ValidContentTypeCount));
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes(Constants.PORTAL_ValidPortalId);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTypeCount, contentTypes.Count());
        }

        [Test]
        public void GetContentTypes_Overload_Calls_Repository_Get_With_PortalId()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var pageIndex = 0;
            var pageSize = 5;
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTypes = contentTypeController.GetContentTypes("Name", portalId, pageIndex, pageSize, false);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Get(portalId));
        }

        [Test]
        public void GetContentTypes_Overload_Calls_Repository_Get_With_HostPortalId_If_IsSystem()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var pageIndex = 0;
            var pageSize = 5;
            var hostPortalId = -1;
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTypes = contentTypeController.GetContentTypes("Name", portalId, pageIndex, pageSize, true);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Get(hostPortalId));
        }

        [Test]
        public void GetContentTypes_Overload_Returns_PagedList()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var pageIndex = 0;
            var pageSize = 5;
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act
            var contentTypes = contentTypeController.GetContentTypes("Name", portalId, pageIndex, pageSize, false);

            //Assert
            Assert.IsInstanceOf<IPagedList<DynamicContentType>>(contentTypes);
        }

        [TestCase(25, 0, "Name", 25, 5)]
        [TestCase(20, 0, "N", 20, 4)]
        [TestCase(150, 0, "nam", 150, 30)]
        [TestCase(10, 0, "_2", 1, 1)]
        public void GetContentTypes_Returns_Correct_ContentTypes(int recordCount, int portalId, string searchTerm, int totalCount, int pageCount)
        {
            //Arrange
            var pageIndex = 0;
            var pageSize = 5;
            _mockContentTypeRepository.Setup(r => r.Get(portalId))
                .Returns(CreateValidContentTypes(recordCount, portalId));
            var dataTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act
            var contentTypes = dataTypeController.GetContentTypes(searchTerm, portalId, pageIndex, pageSize, true);

            //Assert
            Assert.AreEqual(totalCount, contentTypes.TotalCount);
            Assert.AreEqual(pageCount, contentTypes.PageCount);
            if (pageCount > 1)
            {
                Assert.IsTrue(contentTypes.HasNextPage);
            }
            else
            {
                Assert.IsFalse(contentTypes.HasNextPage);
            }
        }

        [Test]
        public void UpdateContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.UpdateContentType(null));
        }

        [Test]
        public void UpdateContentType_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            var contentType = new DynamicContentType { ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void UpdateContentType_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                        {
                                            ContentTypeId = Constants.CONTENTTYPE_InValidContentTypeId,
                                            Name = Constants.CONTENTTYPE_ValidContentType
                                        };

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTypeController.UpdateContentType(contentType));
        }

        [Test]
        public void UpdateContentType_Calls_Repository_Update_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                        {
                                            ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId,
                                            Name = Constants.CONTENTTYPE_UpdateContentType
                                        };

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Update(contentType));
        }

        [Test]
        public void UpdateContentType_Adds_New_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId,
                                        Name = Constants.CONTENTTYPE_UpdateContentType
                                    };

            var fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition());
            }

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockFieldDefinitionController.Verify(fd => fd.AddFieldDefinition(It.IsAny<FieldDefinition>()), Times.Exactly(fieldDefinitionCount));
        }

        [Test]
        public void UpdateContentType_Sets_ContentTypeId_Property_Of_New_New_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId;
            var contentType = new DynamicContentType
                                {
                                    ContentTypeId = contentTypeId,
                                    Name = Constants.CONTENTTYPE_UpdateContentType
                                };

            var fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition());
            }

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreEqual(contentTypeId, field.ContentTypeId);
            }
        }

        [Test]
        public void UpdateContentType_Updates_Existing_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId,
                                        Name = Constants.CONTENTTYPE_UpdateContentType
                                    };

            var fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition() {FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId});
            }

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockFieldDefinitionController.Verify(fd => fd.UpdateFieldDefinition(It.IsAny<FieldDefinition>()), Times.Exactly(fieldDefinitionCount));
        }

        [Test]
        public void UpdateContentType_Adds_New_Templates_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId,
                                        Name = Constants.CONTENTTYPE_UpdateContentType
                                    };

            var contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate());
            }

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockContentTemplateController.Verify(ct => ct.AddContentTemplate(It.IsAny<ContentTemplate>()), Times.Exactly(contentTemplateCount));
        }

        [Test]
        public void UpdateContentType_Sets_ContentTypeId_Property_Of_New_New_Templates_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId;
            var contentType = new DynamicContentType
                                        {
                                            ContentTypeId = contentTypeId,
                                            Name = Constants.CONTENTTYPE_UpdateContentType
                                        };

            var contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate());
            }


            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            foreach (var template in contentType.Templates)
            {
                Assert.AreEqual(contentTypeId, template.ContentTypeId);
            }
        }

        [Test]
        public void UpdateContentType_Updates_Existing_Templates_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = new DynamicContentType
                                        {
                                            ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId,
                                            Name = Constants.CONTENTTYPE_UpdateContentType
                                        };

            var contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate() {TemplateId = Constants.CONTENTTYPE_ValidContentTemplateId});
            }


            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockContentTemplateController.Verify(ct => ct.UpdateContentTemplate(It.IsAny<ContentTemplate>()), Times.Exactly(contentTemplateCount));
        }

        [Test]
        public void UpdateContentType_Updates_LastModifed_Proeprty_On_Valid_ContentType()
        {
            //Arrange
            var userId = Constants.USER_ValidId;
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetCurrentUserInfo()).Returns(new UserInfo { UserID = userId });
            UserController.SetTestableInstance(mockUserController.Object);

            var contentType = new DynamicContentType
                                    {
                                        ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId,
                                        Name = Constants.CONTENTTYPE_UpdateContentType
                                    };

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            Assert.AreEqual(userId, contentType.LastModifiedByUserId);
        }

        private List<DynamicContentType> CreateValidContentTypes(int count)
        {
            var contentTypes = new List<DynamicContentType>();
            for (int i = Constants.CONTENTTYPE_ValidContentTypeId; i < Constants.CONTENTTYPE_ValidContentTypeId + count; i++)
            {
                string contentType = String.Format("Name_{0}", i);

                contentTypes.Add(new DynamicContentType { ContentTypeId = i, Name = contentType });
            }

            return contentTypes;
        }

        private List<DynamicContentType> CreateValidContentTypes(int count, int portalId)
        {
            var contentTypes = new List<DynamicContentType>();
            for (int i = Constants.CONTENTTYPE_ValidContentTypeId; i < Constants.CONTENTTYPE_ValidContentTypeId + count; i++)
            {
                string contentType = String.Format("Name_{0}", i);

                contentTypes.Add(new DynamicContentType { ContentTypeId = i, Name = contentType, PortalId = portalId });
            }

            return contentTypes;
        }
    }
}
