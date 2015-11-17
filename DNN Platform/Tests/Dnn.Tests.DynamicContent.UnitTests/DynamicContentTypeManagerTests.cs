﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Exceptions;
using Dnn.DynamicContent.Localization;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
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
        private Mock<IUserController> _mockUserController;
        
        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);

            _mockDataContext = new Mock<IDataContext>();

            _mockContentTypeRepository = new Mock<IRepository<DynamicContentType>>();
            _mockDataContext.Setup(dc => dc.GetRepository<DynamicContentType>()).Returns(_mockContentTypeRepository.Object);

            _mockFieldDefinitionController = new Mock<IFieldDefinitionManager>();
            _mockFieldDefinitionController.Setup(vr => vr.GetFieldDefinitions(It.IsAny<int>()))
                .Returns(new List<FieldDefinition>().AsQueryable());
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionController.Object);

            _mockUserController = new Mock<IUserController>();
            UserController.SetTestableInstance(_mockUserController.Object);
            _mockUserController.Setup(mu => mu.GetCurrentUserInfo()).Returns(new UserInfo()
            {
                IsSuperUser = false
            });
            
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
            ContentTypeLocalizationManager.ClearInstance();
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

        #region AddContentType tests
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
        public void
            AddContentType_Should_Thrown_SystemContentTypeSecurityException_When_SystemContentTypeIsAddedByNonSuperUser()
        {
            // Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            var contentType = GetValidContentType();
            contentType.PortalId = Null.NullInteger; // Means system content type

            // Act
            var act = new TestDelegate(() => contentTypeController.AddContentType(contentType));

            // Assert
            Assert.Throws<SystemContentTypeSecurityException>(act);
        }

        [Test]
        public void AddContentType_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            var contentType = GetValidNewContentType();

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
            var contentType = GetValidNewContentType();
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);


            //Act
            int contentTypeId = contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentTypeId);
        }

        [Test]
        public void AddContentType_Sets_ValidId_On_Valid_ContentType()
        {
            //Arrange
            var contentType = GetValidNewContentType();
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);


            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTypeId, contentType.ContentTypeId);
        }

        [Test]
        public void AddContentType_Adds_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            var contentType = GetValidNewContentType();
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);


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
            var contentType = GetValidNewContentType();
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            
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
                Assert.AreEqual(contentType.ContentTypeId, field.ContentTypeId);
            }
        }

        [Test]
        public void AddContentType_Adds_ContentTemplates_On_Valid_ContentType()
        {
            //Arrange
            var contentType = GetValidNewContentType();
            _mockContentTypeRepository.Setup(r => r.Insert(It.IsAny<DynamicContentType>()))
                            .Callback((DynamicContentType ct) => ct.ContentTypeId = Constants.CONTENTTYPE_AddContentTypeId);

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);


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

            var contentType = GetValidNewContentType();

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

            var contentType = GetValidNewContentType();

            //Act
            contentTypeController.AddContentType(contentType);

            //Assert
            Assert.AreEqual(userId, contentType.CreatedByUserId);
        }
        #endregion

        #region DeleteContentType tests
        [Test]
        public void DeleteContentType_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTypeController.DeleteContentType(null));
        }

        [Test]
        public void DeleteContentType_ThrowsException_WhenContentTypeIsInUseByOtherContentType()
        {
            //Arrange
            var selectContentTypeInUseByOtherContentType = @"SELECT count(*)
                                                        FROM {objectQualifier}ContentTypes_FieldDefinitions
                                                        WHERE IsReferenceType = 1 AND FieldTypeID = " +
                                                                 Constants.CONTENTTYPE_ValidContentTypeId;

            _mockDataContext.Setup(dc => dc.ExecuteScalar<int>(It.IsAny<CommandType>(), It.IsAny<string>() )).Returns(0);
            _mockDataContext.Setup(dc => dc.ExecuteScalar<int>(It.IsAny<CommandType>(), selectContentTypeInUseByOtherContentType)).Returns(1);

            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(CreateValidContentTypes(Constants.CONTENTTYPE_ValidContentTypeCount));

            _mockUserController.Setup(mu => mu.GetCurrentUserInfo()).Returns(new UserInfo()
            {
                IsSuperUser = true
            });

            var contentType = new DynamicContentType
            {
                Name = Constants.CONTENTTYPE_ValidContentType,
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                PortalId = Constants.PORTAL_ValidPortalId
            };

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ContentTypeInUseException>(() => contentTypeController.DeleteContentType(contentType));
        }

        [Test]
        public void DeleteContentType_ThrowsException_WhenContentTypeIsInUseByContentItems()
        {
            //Arrange
            var selectContentTypeInUseByContentItems = @"SELECT count(*)
                                                        FROM {objectQualifier}ContentItems
                                                        WHERE ContentTypeID = " +
                                                              Constants.CONTENTTYPE_ValidContentTypeId;

            _mockDataContext.Setup(dc => dc.ExecuteScalar<int>(It.IsAny<CommandType>(), It.IsAny<string>() )).Returns(0);
            _mockDataContext.Setup(dc => dc.ExecuteScalar<int>(It.IsAny<CommandType>(), selectContentTypeInUseByContentItems)).Returns(1);

            _mockContentTypeRepository.Setup(r => r.Get(Constants.PORTAL_ValidPortalId))
                .Returns(CreateValidContentTypes(Constants.CONTENTTYPE_ValidContentTypeCount));

            _mockUserController.Setup(mu => mu.GetCurrentUserInfo()).Returns(new UserInfo()
            {
                IsSuperUser = true
            });

            var contentType = new DynamicContentType
            {
                Name = Constants.CONTENTTYPE_ValidContentType,
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId,
                PortalId = Constants.PORTAL_ValidPortalId
            };

            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ContentTypeInUseException>(() => contentTypeController.DeleteContentType(contentType));
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
        public void
            DeleteContentType_Should_Thrown_SystemContentTypeSecurityException_When_SystemContentTypeIsDeletedByNonSuperUser()
        {
            // Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            var contentType = GetValidContentType();
            contentType.PortalId = Null.NullInteger; // Means system content type
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] {contentType});

            // Act
            var act = new TestDelegate(() => contentTypeController.DeleteContentType(contentType));

            // Assert
            Assert.Throws<SystemContentTypeSecurityException>(act);
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void DeleteContentType_Throws_ContentTypeDoesNotExistException_When_Type_Does_Not_Exsist()
        {
            // Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = GetValidContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new DynamicContentType[0]);

            // Act / Assert            
            Assert.Throws<DynamicContentTypeDoesNotExistException>(() => contentTypeController.DeleteContentType(contentType));
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void DeleteContentType_Calls_Repository_Delete_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            var contentType = GetValidContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            var mockLocalization = new Mock<IContentTypeLocalizationManager>();
            ContentTypeLocalizationManager.SetTestableInstance(mockLocalization.Object);

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Delete(contentType));
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void DeleteContentType_Deletes_FieldDefinitions_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            var contentType = GetValidContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            var mockLocalization = new Mock<IContentTypeLocalizationManager>();
            ContentTypeLocalizationManager.SetTestableInstance(mockLocalization.Object);

            var fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition());
            }

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockFieldDefinitionController.Verify(f => f.DeleteFieldDefinition(It.IsAny<FieldDefinition>()), Times.Exactly(fieldDefinitionCount));
            _mockContentTypeRepository.VerifyAll();

        }

        [Test]
        public void DeleteContentType_Deletes_Templates_On_Valid_ContentTypeId()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            var contentType = GetValidContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            var mockLocalization = new Mock<IContentTypeLocalizationManager>();
            ContentTypeLocalizationManager.SetTestableInstance(mockLocalization.Object);

            var contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate());
            }

            //Act
            contentTypeController.DeleteContentType(contentType);

            //Assert
            _mockContentTemplateController.Verify(ct => ct.DeleteContentTemplate(It.IsAny<ContentTemplate>()), Times.Exactly(contentTemplateCount));
            _mockContentTypeRepository.VerifyAll();
        }
        #endregion

        #region GetContentTypes tests
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
        #endregion

        #region UpdateContentType tests
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
        public void
            UpdateContentType_Should_Thrown_SystemContentTypeSecurityException_When_SystemContentTypeIsModifiedByNonSuperUser()
        {
            // Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            var contentType = GetValidUpdateContentType();
            contentType.PortalId = Null.NullInteger; // Means system content type
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            // Act
            var act = new TestDelegate(() => contentTypeController.UpdateContentType(contentType));

            // Assert
            Assert.Throws<SystemContentTypeSecurityException>(act);
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Calls_Repository_Update_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockContentTypeRepository.Verify(r => r.Update(contentType));
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Adds_New_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            const int fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition());
            }

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockFieldDefinitionController.Verify(fd => fd.AddFieldDefinition(It.IsAny<FieldDefinition>()), Times.Exactly(fieldDefinitionCount));
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Sets_ContentTypeId_Property_Of_New_New_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            const int fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition());
            }

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            foreach (var field in contentType.FieldDefinitions)
            {
                Assert.AreEqual(contentType.ContentTypeId, field.ContentTypeId);
            }
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Updates_Existing_FieldDefinitions_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            const int fieldDefinitionCount = 5;
            for (int i = 0; i < fieldDefinitionCount; i++)
            {
                contentType.FieldDefinitions.Add(new FieldDefinition {FieldDefinitionId = Constants.CONTENTTYPE_ValidFieldDefinitionId});
            }

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockFieldDefinitionController.Verify(fd => fd.UpdateFieldDefinition(It.IsAny<FieldDefinition>()), Times.Exactly(fieldDefinitionCount));
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Adds_New_Templates_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            var contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate());
            }

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockContentTemplateController.Verify(ct => ct.AddContentTemplate(It.IsAny<ContentTemplate>()), Times.Exactly(contentTemplateCount));
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Sets_ContentTypeId_Property_Of_New_New_Templates_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            const int contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate());
            }


            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            foreach (var template in contentType.Templates)
            {
                Assert.AreEqual(contentType.ContentTypeId, template.ContentTypeId);
            }
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Updates_Existing_Templates_On_Valid_ContentType()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            const int contentTemplateCount = 5;
            for (int i = 0; i < contentTemplateCount; i++)
            {
                contentType.Templates.Add(new ContentTemplate {TemplateId = Constants.CONTENTTYPE_ValidContentTemplateId});
            }


            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            _mockContentTemplateController.Verify(ct => ct.UpdateContentTemplate(It.IsAny<ContentTemplate>()), Times.Exactly(contentTemplateCount));
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Updates_LastModifed_Proeprty_On_Valid_ContentType()
        {
            //Arrange
            const int userId = Constants.USER_ValidId;
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetCurrentUserInfo()).Returns(new UserInfo { UserID = userId });
            UserController.SetTestableInstance(mockUserController.Object);

            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new[] { contentType });

            //Act
            contentTypeController.UpdateContentType(contentType);

            //Assert
            Assert.AreEqual(userId, contentType.LastModifiedByUserId);
            _mockContentTypeRepository.VerifyAll();
        }

        [Test]
        public void UpdateContentType_Throws_ContentTypeDoesNotExistExecption_When_Type_Does_Not_Exist()
        {
            //Arrange
            var contentTypeController = new DynamicContentTypeManager(_mockDataContext.Object);
            
            var contentType = GetValidUpdateContentType();
            _mockContentTypeRepository.Setup(r => r.Get(contentType.PortalId)).Returns(new DynamicContentType[0]);

            //Act / Assert
            Assert.Throws<DynamicContentTypeDoesNotExistException>(() => contentTypeController.UpdateContentType(contentType));
            _mockContentTypeRepository.VerifyAll();
        }

        #endregion

        private DynamicContentType GetValidContentType()
        {
            return new DynamicContentType
            {
                Name = Constants.CONTENTTYPE_ValidContentType,
                PortalId = 0,
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
            };
        }

        private DynamicContentType GetValidUpdateContentType()
        {
            return new DynamicContentType
            {
                Name = Constants.CONTENTTYPE_UpdateContentType,
                PortalId = 0,
                ContentTypeId = Constants.CONTENTTYPE_UpdateContentTypeId
            };
        }

        private DynamicContentType GetValidNewContentType()
        {
            return new DynamicContentType
            {
                Name = Constants.CONTENTTYPE_ValidContentType,
                PortalId = 0
            };
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
