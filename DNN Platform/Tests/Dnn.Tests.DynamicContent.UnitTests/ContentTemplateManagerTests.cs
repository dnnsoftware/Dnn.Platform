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

// ReSharper disable UseStringInterpolation

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class ContentTemplateManagerTests
    {
        private Mock<IDataContext> _mockDataContext;
        private Mock<IRepository<ContentTemplate>> _mockContentTemplateRepository;
        // ReSharper disable once NotAccessedField.Local
        private Mock<CachingProvider> _mockCache;

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);

            _mockDataContext = new Mock<IDataContext>();

            _mockContentTemplateRepository = new Mock<IRepository<ContentTemplate>>();
            _mockDataContext.Setup(dc => dc.GetRepository<ContentTemplate>()).Returns(_mockContentTemplateRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            ContentController.ClearInstance();
        }

        [Test]
        public void AddContentTemplate_Throws_On_Null_ContentTemplate()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTemplateController.AddContentTemplate(null));
        }

        [Test]
        public void AddContentTemplate_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);
            var contentTemplate = new ContentTemplate()
                                        {
                                            Name = String.Empty,
                                            TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId,
                                            ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                        };


            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTemplateController.AddContentTemplate(contentTemplate));
        }

        [Test]
        public void AddContentTemplate_Throws_On_Negative_ContentTypeId_Property()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);
            var contentTemplate = new ContentTemplate()
                                        {
                                            Name = "Name",
                                            TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId,
                                            ContentTypeId = Constants.CONTENTTYPE_InValidContentTypeId
                                        };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTemplateController.AddContentTemplate(contentTemplate));
        }

        [Test]
        public void AddContentTemplate_Calls_Repository_Insert_On_Valid_Arguments()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate()
                                        {
                                            Name = "Name",
                                            TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId,
                                            ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                        };

            //Act
            // ReSharper disable once UnusedVariable
            int contentTemplateId = contentTemplateController.AddContentTemplate(contentTemplate);

            //Assert
            _mockContentTemplateRepository.Verify(rep => rep.Insert(contentTemplate));
        }

        [Test]
        public void AddContentTemplate_Returns_ValidId_On_Valid_ContentTemplate()
        {
            //Arrange
            _mockContentTemplateRepository.Setup(r => r.Insert(It.IsAny<ContentTemplate>()))
                            .Callback((ContentTemplate dt) => dt.TemplateId = Constants.CONTENTTYPE_AddContentTemplateId);

            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate()
                                        {
                                            Name = "Name",
                                            TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId,
                                            ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                        };

            //Act
            int contentTemplateId = contentTemplateController.AddContentTemplate(contentTemplate);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTemplateId, contentTemplateId);
        }

        [Test]
        public void AddContentTemplate_Sets_ValidId_On_Valid_ContentTemplate()
        {
            //Arrange
            _mockContentTemplateRepository.Setup(r => r.Insert(It.IsAny<ContentTemplate>()))
                            .Callback((ContentTemplate dt) => dt.TemplateId = Constants.CONTENTTYPE_AddContentTemplateId);

            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate()
                                        {
                                            Name = "Name",
                                            TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId,
                                            ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                        };

            //Act
            contentTemplateController.AddContentTemplate(contentTemplate);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_AddContentTemplateId, contentTemplate.TemplateId);
        }

        [Test]
        public void AddContentTemplate_Sets_Created_Audit_Info_On_Valid_ContentTemplate()
        {
            //Arrange
            var userId = Constants.USER_ValidId;
            _mockContentTemplateRepository.Setup(r => r.Insert(It.IsAny<ContentTemplate>()))
                            .Callback((ContentTemplate dt) => dt.TemplateId = Constants.CONTENTTYPE_AddContentTemplateId);

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetCurrentUserInfo()).Returns(new UserInfo { UserID = userId });
            UserController.SetTestableInstance(mockUserController.Object);

            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate()
            {
                Name = "Name",
                TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId,
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
            };

            //Act
            contentTemplateController.AddContentTemplate(contentTemplate);

            //Assert
            Assert.AreEqual(userId, contentTemplate.CreatedByUserId);
        }

        [Test]
        public void DeleteContentTemplate_Throws_On_Null_ContentTemplate()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTemplateController.DeleteContentTemplate(null));
        }

        [Test]
        public void DeleteContentTemplate_Throws_On_Negative_TemplateId()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate { TemplateId = Null.NullInteger };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTemplateController.DeleteContentTemplate(contentTemplate));
        }

        [Test]
        public void DeleteContentTemplate_Calls_Repository_Delete_If_Valid()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate { TemplateId = Constants.CONTENTTYPE_ValidContentTemplateId };

            //Act
            contentTemplateController.DeleteContentTemplate(contentTemplate);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Delete(contentTemplate));
        }

        [Test]
        public void GetContentTemplates_Calls_Repository_Get_With_PortalId()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTemplates = contentTemplateController.GetContentTemplates(portalId);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Get(portalId));
        }

        [Test]
        public void GetContentTemplates_Calls_Repository_Get_With_Host_PortalId_If_IncludeSystem_True()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var hostPortalId = -1;
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTemplates = contentTemplateController.GetContentTemplates(portalId, true);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Get(hostPortalId));
        }

        [Test]
        public void GetContentTemplates_Returns_Empty_List_Of_ContentTemplates_If_No_ContentTemplates()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            _mockContentTemplateRepository.Setup(r => r.Get(portalId))
                .Returns(GetValidContentTemplates(0, 0));
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(portalId);

            //Assert
            Assert.IsNotNull(contentTemplates);
            Assert.AreEqual(0, contentTemplates.Count());
        }

        [Test]
        public void GetContentTemplates_Returns_List_Of_ContentTemplates()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            _mockContentTemplateRepository.Setup(r => r.Get(portalId))
                .Returns(GetValidContentTemplates(Constants.CONTENTTYPE_ValidContentTemplateCount, Constants.CONTENTTYPE_ValidContentTypeId));
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(portalId);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTemplateCount, contentTemplates.Count());
        }

        [Test]
        public void GetContentTemplates_Overload_Calls_Repository_Get_With_PortalId()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId, portalId);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Get(portalId));
        }

        [Test]
        public void GetContentTemplates_Overload_Calls_Repository_Get_With_Host_PortalId_If_IncludeSystem_True()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var hostPortalId = -1;
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId, portalId, true);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Get(hostPortalId));
        }

        [Test]
        public void GetContentTemplatess_Overload_Returns_Empty_List_Of_ContentTemplates_If_No_ContentTemplates()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;
            _mockContentTemplateRepository.Setup(r => r.Get(portalId))
                .Returns(GetValidContentTemplates(0, contentTypeId));
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId, portalId);

            //Assert
            Assert.IsNotNull(contentTemplates);
            Assert.AreEqual(0, contentTemplates.Count());
        }

        [Test]
        public void GetContentTemplates_Overload_Returns_List_Of_ContentTemplatess()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var portalId = Constants.PORTAL_ValidPortalId;
            _mockContentTemplateRepository.Setup(r => r.Get(portalId))
                .Returns(GetValidContentTemplates(Constants.CONTENTTYPE_ValidContentTemplateCount, contentTypeId));
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId, portalId);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTemplateCount, contentTemplates.Count());
        }

        [Test]
        public void GetContentTemplates_Second_Overload_Calls_Repository_Get_With_PortalId()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var pageIndex = 0;
            var pageSize = 5;
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTemplates = contentTemplateController.GetContentTemplates("Search", portalId, pageIndex, pageSize, false);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Get(portalId));
        }

        [Test]
        public void GetContentTemplates_Second_Overload_Calls_Repository_Get_With_Host_PortalId_If_IncludeSystem_True()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var pageIndex = 0;
            var pageSize = 5;
            var hostPortalId = -1;
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTemplates = contentTemplateController.GetContentTemplates("Search", portalId, pageIndex, pageSize, true);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Get(hostPortalId));
        }

        [Test]
        public void GetContentTemplates_Second_Overload_Returns_PagedList()
        {
            //Arrange
            var portalId = Constants.PORTAL_ValidPortalId;
            var pageIndex = 0;
            var pageSize = 5;
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTemplates = contentTemplateController.GetContentTemplates("Name", portalId, pageIndex, pageSize, true);

            //Assert
            Assert.IsInstanceOf<IPagedList<ContentTemplate>>(contentTemplates);
        }

        [TestCase(25, 0, "Name", 25, 5)]
        [TestCase(20, 0, "N", 20, 4)]
        [TestCase(150, 0, "nam", 150, 30)]
        [TestCase(10, 0, "_2", 1, 1)]
        public void GetContentTemplates_Second_Returns_Correct_ContentTemplatess(int recordCount, int portalId, string searchTerm, int totalCount, int pageCount)
        {
            //Arrange
            var pageIndex = 0;
            var pageSize = 5;
            _mockContentTemplateRepository.Setup(r => r.Get(portalId))
                .Returns(GetValidContentTemplatesByPortal(recordCount, portalId));
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(searchTerm, portalId, pageIndex, pageSize, true);

            //Assert
            Assert.AreEqual(totalCount, contentTemplates.TotalCount);
            Assert.AreEqual(pageCount, contentTemplates.PageCount);
            if (pageCount > 1)
            {
                Assert.IsTrue(contentTemplates.HasNextPage);
            }
            else
            {
                Assert.IsFalse(contentTemplates.HasNextPage);
            }
        }

        [Test]
        public void UpdateContentTemplate_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTemplateController.UpdateContentTemplate(null));
        }

        [Test]
        public void UpdateContentTemplate_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);
            var contentTemplate = new ContentTemplate { TemplateId = Constants.CONTENTTYPE_ValidContentTemplateId };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTemplateController.UpdateContentTemplate(contentTemplate));
        }

        [Test]
        public void UpdateContentTemplate_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate()
                                            {
                                                Name = "Name",
                                                TemplateId = Constants.CONTENTTYPE_UpdateContentTemplateId,
                                                ContentTypeId = Constants.CONTENTTYPE_InValidContentTypeId
                                            };

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTemplateController.UpdateContentTemplate(contentTemplate));
        }

        [Test]
        public void UpdateContentTemplate_Throws_On_Negative_ContentTemplateId()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate { TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId, Name = "Test" };

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTemplateController.UpdateContentTemplate(contentTemplate));
        }

        [Test]
        public void UpdateContentTemplate_Calls_Repository_Update_If_ContentTemplate_Is_Valid()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate()
                                        {
                                            Name = "Name",
                                            TemplateId = Constants.CONTENTTYPE_UpdateContentTemplateId,
                                            ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                        };

            //Act
            contentTemplateController.UpdateContentTemplate(contentTemplate);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Update(contentTemplate));
        }

        [Test]
        public void UpdateContentTemplate_Sets_Updated_Audit_Info_If_ContentTemplate_Is_Valid()
        {
            //Arrange
            var userId = Constants.USER_ValidId;
            var contentTemplateController = new ContentTemplateManager(_mockDataContext.Object);

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetCurrentUserInfo()).Returns(new UserInfo { UserID = userId });
            UserController.SetTestableInstance(mockUserController.Object);


            var contentTemplate = new ContentTemplate()
            {
                Name = "Name",
                TemplateId = Constants.CONTENTTYPE_UpdateContentTemplateId,
                ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
            };

            //Act
            contentTemplateController.UpdateContentTemplate(contentTemplate);

            //Assert
            Assert.AreEqual(userId, contentTemplate.LastModifiedByUserId);
        }

        private List<ContentTemplate> GetValidContentTemplates(int count, int contentTypeId)
        {
            var list = new List<ContentTemplate>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new ContentTemplate() { ContentTypeId = contentTypeId, TemplateId = i, Name = String.Format("Name_{0}", i) });
            }

            return list;
        }

        private List<ContentTemplate> GetValidContentTemplatesByPortal(int count, int portalId)
        {
            var list = new List<ContentTemplate>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new ContentTemplate()
                {
                    ContentTypeId = i,
                    TemplateId = i,
                    Name = String.Format("Name_{0}", i),
                    PortalId = portalId
                });
            }

            return list;
        }
    }
}
