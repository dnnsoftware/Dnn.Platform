#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class ContentTemplateControllerTests
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
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTemplateController.AddContentTemplate(null));
        }

        [Test]
        public void AddContentTemplate_Throws_On_Empty_Name_Property()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);
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
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);
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
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

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

            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

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

            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

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
        public void DeleteContentTemplate_Throws_On_Null_ContentTemplate()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTemplateController.DeleteContentTemplate(null));
        }

        [Test]
        public void DeleteContentTemplate_Throws_On_Negative_TemplateId()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate { TemplateId = Null.NullInteger };

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => contentTemplateController.DeleteContentTemplate(contentTemplate));
        }

        [Test]
        public void DeleteContentTemplate_Calls_Repository_Delete_If_Valid()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate { TemplateId = Constants.CONTENTTYPE_ValidContentTemplateId };

            //Act
            contentTemplateController.DeleteContentTemplate(contentTemplate);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Delete(contentTemplate));
        }

        [Test]
        public void GetContentTemplates_Calls_Repository_Get()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            //Act
            // ReSharper disable once UnusedVariable
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId);

            //Assert
            _mockContentTemplateRepository.Verify(r => r.Get(contentTypeId));
        }

        [Test]
        public void GetContentTemplatess_Returns_Empty_List_Of_ContentTypes_If_No_ContentTypes()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            _mockContentTemplateRepository.Setup(r => r.Get(contentTypeId))
                .Returns(GetValidContentTemplates(0));
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId);

            //Assert
            Assert.IsNotNull(contentTemplates);
            Assert.AreEqual(0, contentTemplates.Count());
        }

        [Test]
        public void GetContentTemplates_Returns_List_Of_ContentTypes()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            _mockContentTemplateRepository.Setup(r => r.Get(contentTypeId))
                .Returns(GetValidContentTemplates(Constants.CONTENTTYPE_ValidContentTemplateCount));
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId);

            //Assert
            Assert.AreEqual(Constants.CONTENTTYPE_ValidContentTemplateCount, contentTemplates.Count());
        }

        [Test]
        public void UpdateContentTemplate_Throws_On_Null_ContentType()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => contentTemplateController.UpdateContentTemplate(null));
        }

        [Test]
        public void UpdateContentTemplate_Throws_On_Empty_ContentType_Property()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);
            var contentTemplate = new ContentTemplate { TemplateId = Constants.CONTENTTYPE_ValidContentTemplateId };

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => contentTemplateController.UpdateContentTemplate(contentTemplate));
        }

        [Test]
        public void UpdateContentTemplate_Throws_On_Negative_ContentTypeId()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

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
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

            var contentTemplate = new ContentTemplate { TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId, Name = "Test" };

            Assert.Throws<ArgumentOutOfRangeException>(() => contentTemplateController.UpdateContentTemplate(contentTemplate));
        }

        [Test]
        public void UpdateContentTemplate_Calls_Repository_Update_If_ContentTemplate_Is_Valid()
        {
            //Arrange
            var contentTemplateController = new ContentTemplateController(_mockDataContext.Object);

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

        private List<ContentTemplate> GetValidContentTemplates(int count)
        {
            var list = new List<ContentTemplate>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new ContentTemplate() { TemplateId = i, Name = String.Format("Name_{0}", i) });
            }

            return list;
        }
    }
}
