// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Data;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

// ReSharper disable UseStringInterpolation
// ReSharper disable BuiltInTypeReferenceStyle

namespace Dnn.Tests.DynamicContent.IntegrationTests
{
    [TestFixture]
    public class ContentTemplateIntegrationTests : IntegrationTestBase
    {
        private readonly string _cacheKey = CachingProvider.GetCacheKey(ContentTemplateController.ContentTemplateCacheKey);

        [SetUp]
        public void SetUp()
        {
            SetUpInternal();
        }

        [TearDown]
        public void TearDown()
        {
            TearDownInternal();
            ContentController.ClearInstance();
        }

        [Test]
        public void AddContentTemplate_Inserts_New_Record_In_Database()
        {
            //Arrange
            SetUpContentTemplates(RecordCount);
            var contentTemplateController = new ContentTemplateController();
            var contentTemplate = new ContentTemplate()
                                        {
                                            Name = "Name",
                                            TemplateId = Constants.CONTENTTYPE_InValidContentTemplateId,
                                            ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                        };

            //Act
            contentTemplateController.AddContentTemplate(contentTemplate);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_Templates");

            Assert.AreEqual(RecordCount + 1, actualCount);
        }

        [Test]
        public void AddContentTemplate_Clears_Cache()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            SetUpContentTemplates(RecordCount);
            var contentTemplateController = new ContentTemplateController();
            var contentTemplate = new ContentTemplate() { ContentTypeId = contentTypeId, Name = "New_Type" };

            //Act
            contentTemplateController.AddContentTemplate(contentTemplate);

            //Assert
            MockCache.Verify(c => c.Remove(GetCacheKey(contentTypeId)));
        }

        [Test]
        public void DeleteContentTemplate_Deletes_Record_From_Database()
        {
            //Arrange
            var templateId = 6;
            SetUpContentTemplates(RecordCount);

            var contentTemplateController = new ContentTemplateController();
            var contentTemplate = new ContentTemplate() { TemplateId = templateId, Name = "New_Type" };

            //Act
            contentTemplateController.DeleteContentTemplate(contentTemplate);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_Templates");

            Assert.AreEqual(RecordCount - 1, actualCount);
        }

        [Test]
        public void DeleteContentTemplate_Clears_Cache()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var templateId = 6;
            SetUpContentTemplates(RecordCount);

            var contentTemplateController = new ContentTemplateController();
            var contentTemplate = new ContentTemplate() { ContentTypeId = contentTypeId, TemplateId = templateId, Name = "New_Type" };

            //Act
            contentTemplateController.DeleteContentTemplate(contentTemplate);

            //Assert
            MockCache.Verify(c => c.Remove(GetCacheKey(contentTypeId)));
        }

        [Test]
        public void GetContentTemplates_Fetches_Records_From_Database_If_Cache_Is_Null()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            MockCache.Setup(c => c.GetItem(GetCacheKey(contentTypeId))).Returns(null);
            SetUpContentTemplates(RecordCount);

            var contentTemplateController = new ContentTemplateController();

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId);

            //Assert
            Assert.AreEqual(1, contentTemplates.Count());
        }

        [Test]
        public void GetContentTemplates_Fetches_Records_From_Cache_If_Not_Null()
        {
            //Arrange
            var cacheCount = 15;
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            MockCache.Setup(c => c.GetItem(GetCacheKey(contentTypeId))).Returns(SetUpCache(cacheCount));

            SetUpContentTemplates(RecordCount);

            var contentTemplateController = new ContentTemplateController();

            //Act
            var contentTemplates = contentTemplateController.GetContentTemplates(contentTypeId);

            //Assert
            Assert.AreEqual(cacheCount, contentTemplates.Count());
        }

        [Test]
        public void UpdateContentTemplate_Updates_Correct_Record_In_Database()
        {
            //Arrange
            var templateId = 2;
            SetUpContentTemplates(RecordCount);

            var contentTemplateController = new ContentTemplateController();
            var contentTemplate = new ContentTemplate()
                                        {
                                            Name = "Name",
                                            TemplateId = templateId,
                                            ContentTypeId = Constants.CONTENTTYPE_ValidContentTypeId
                                        };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                    .Returns(new List<ContentItem>().AsQueryable());

            //Act
            contentTemplateController.UpdateContentTemplate(contentTemplate);

            //Assert
            int actualCount = DataUtil.GetRecordCount(DatabaseName, "ContentTypes_Templates");
            Assert.AreEqual(RecordCount, actualCount);

            DataAssert.IsFieldValueEqual("Name", DatabaseName, "ContentTypes_Templates", "Name", "TemplateId", templateId);
        }

        [Test]
        public void UpdateContentTemplate_Clears_Cache()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;
            var templateId = 2;
            SetUpContentTemplates(RecordCount);

            var contentTemplateController = new ContentTemplateController();
            var contentTemplate = new ContentTemplate() { ContentTypeId = contentTypeId, TemplateId = templateId, Name = "NewType" };

            var mockContentController = new Mock<IContentController>();
            ContentController.SetTestableInstance(mockContentController.Object);
            mockContentController.Setup(c => c.GetContentItemsByContentType(Constants.CONTENTTYPE_ValidContentTypeId))
                    .Returns(new List<ContentItem>().AsQueryable());

            //Act
            contentTemplateController.UpdateContentTemplate(contentTemplate);

            //Assert
            MockCache.Verify(c => c.Remove(GetCacheKey(contentTypeId)));
        }

        private string GetCacheKey(int contentTypeId)
        {
            return String.Format("{0}_{1}_{2}", _cacheKey, ContentTemplateController.ContentTemplateScope, contentTypeId);
        }

        private IQueryable<ContentTemplate> SetUpCache(int count)
        {
            var list = new List<ContentTemplate>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new ContentTemplate { TemplateId = i, Name = String.Format("Type_{0}", i) });
            }
            return list.AsQueryable();
        }
    }
}
