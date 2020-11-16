// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Content.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Web.Validators;
    using Moq;

    using FileController = DotNetNuke.Entities.Content.AttachmentController;

    public static class MockHelper
    {
        internal static IQueryable<Vocabulary> TestVocabularies
        {
            get
            {
                List<Vocabulary> vocabularies = new List<Vocabulary>();

                for (int i = Constants.VOCABULARY_ValidVocabularyId; i < Constants.VOCABULARY_ValidVocabularyId + Constants.VOCABULARY_ValidCount; i++)
                {
                    Vocabulary vocabulary = new Vocabulary();
                    vocabulary.VocabularyId = i;
                    vocabulary.Name = ContentTestHelper.GetVocabularyName(i);
                    vocabulary.Type = (i == Constants.VOCABULARY_HierarchyVocabularyId) ? VocabularyType.Hierarchy : VocabularyType.Simple;
                    vocabulary.Description = ContentTestHelper.GetVocabularyName(i);
                    vocabulary.ScopeTypeId = Constants.SCOPETYPE_ValidScopeTypeId;
                    vocabulary.Weight = Constants.VOCABULARY_ValidWeight;

                    vocabularies.Add(vocabulary);
                }

                return vocabularies.AsQueryable();
            }
        }

        internal static IDataReader CreateEmptyContentItemReader()
        {
            return CreateContentItemTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyContentTypeReader()
        {
            return CreateContentTypeTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyScopeTypeReader()
        {
            return CreateScopeTypeTable().CreateDataReader();
        }

        internal static IDataReader CreateEmptyTermReader()
        {
            return CreateTermTable().CreateDataReader();
        }

        internal static Mock<HttpContextBase> CreateMockHttpContext()
        {
            Mock<HttpContextBase> httpContext = new Mock<HttpContextBase>();
            Mock<HttpResponseBase> httpResponse = new Mock<HttpResponseBase>();
            httpContext.Setup(h => h.Response).Returns(httpResponse.Object);

            return httpContext;
        }

        internal static Mock<IVocabularyController> CreateMockVocabularyController()
        {
            // Create the mock
            var mockVocabularies = new Mock<IVocabularyController>();
            mockVocabularies.Setup(v => v.GetVocabularies()).Returns(TestVocabularies);

            // Register Mock
            return RegisterMockController(mockVocabularies);
        }

        internal static IDataReader CreateValidContentItemReader()
        {
            DataTable table = CreateContentItemTable();
            AddContentItemToTable(
                table,
                Constants.CONTENT_ValidContentItemId,
                ContentTestHelper.GetContent(Constants.CONTENT_ValidContentItemId),
                ContentTestHelper.GetContentKey(Constants.CONTENT_ValidContentItemId),
                true,
                Constants.USER_ValidId,
                Null.NullString);

            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidContentItemReader(ContentItem contentItem)
        {
            DataTable table = CreateContentItemTable();

            AddContentItemToTable(
                table,
                contentItem.ContentItemId,
                ContentTestHelper.GetContent(contentItem.ContentItemId),
                ContentTestHelper.GetContentKey(contentItem.ContentItemId),
                true,
                Constants.USER_ValidId,
                Null.NullString);

            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidContentItemsReader(int count, bool indexed, int startUserId, string term)
        {
            DataTable table = CreateContentItemTable();
            for (int i = Constants.CONTENT_ValidContentItemId; i < Constants.CONTENT_ValidContentItemId + count; i++)
            {
                string content = (count == 1) ? Constants.CONTENT_ValidContent : ContentTestHelper.GetContent(i);
                string contentKey = (count == 1) ? Constants.CONTENT_ValidContentKey : ContentTestHelper.GetContentKey(i);
                int userId = (startUserId == Null.NullInteger) ? Constants.USER_ValidId + i : startUserId;

                AddContentItemToTable(table, i, content, contentKey, indexed, startUserId, term);
            }

            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidContentTypesReader(int count)
        {
            DataTable table = CreateContentTypeTable();
            for (int i = Constants.CONTENTTYPE_ValidContentTypeId; i < Constants.CONTENTTYPE_ValidContentTypeId + count; i++)
            {
                string contentType = (count == 1) ? Constants.CONTENTTYPE_ValidContentType : ContentTestHelper.GetContentType(i);

                AddContentTypeToTable(table, i, contentType);
            }

            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidScopeTypesReader(int count)
        {
            DataTable table = CreateScopeTypeTable();
            for (int i = Constants.SCOPETYPE_ValidScopeTypeId; i < Constants.SCOPETYPE_ValidScopeTypeId + count; i++)
            {
                string scopeType = (count == 1) ? Constants.SCOPETYPE_ValidScopeType : ContentTestHelper.GetScopeType(i);

                AddScopeTypeToTable(table, i, scopeType);
            }

            return table.CreateDataReader();
        }

        internal static DataTable CreateMetaDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("MetaDataName", typeof(string));
            table.Columns.Add("MetaDataValue", typeof(string));

            return table;
        }

        internal static IDataReader CreateValidMetaDataReader()
        {
            // Create Categories table.
            using (var table = CreateMetaDataTable())
            {
                for (int i = 0; i < Constants.CONTENT_MetaDataCount; i++)
                {
                    table.Rows.Add(new object[] { string.Format("{0} {1}", Constants.CONTENT_ValidMetaDataName, i), Constants.CONTENT_ValidMetaDataValue });
                }

                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateMetaDataReaderWithFiles(IList<IFileInfo> files, IList<IFileInfo> videos, IList<IFileInfo> images)
        {
            using (var table = CreateMetaDataTable())
            {
                if (files.Any())
                {
                    table.Rows.Add(FileController.FilesKey, FileController.SerializeFileInfo(files));
                }

                if (videos.Any())
                {
                    table.Rows.Add(FileController.VideoKey, FileController.SerializeFileInfo(videos));
                }

                if (images.Any())
                {
                    table.Rows.Add(FileController.ImageKey, FileController.SerializeFileInfo(images));
                }

                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateMetaDataReaderFromDictionary(IDictionary<string, string> map)
        {
            using (var table = CreateMetaDataTable())
            {
                foreach (var kvp in map)
                {
                    table.Rows.Add(kvp.Key, kvp.Value);
                }

                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateEmptyMetaDataReader()
        {
            using (var table = CreateMetaDataTable())
            {
                return table.CreateDataReader();
            }
        }

        internal static IDataReader CreateValidTermReader()
        {
            DataTable table = CreateTermTable();
            AddTermToTable(
                table,
                Constants.TERM_ValidTermId,
                Constants.TERM_ValidContent1,
                Constants.TERM_ValidVocabulary1,
                Constants.TERM_ValidName,
                Constants.TERM_ValidName,
                Constants.TERM_ValidWeight,
                Constants.TERM_ValidParentTermId);

            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidTermsReader(int count, Func<int, int> vocabularyIdFunction, Func<int, int> contentIdFunction)
        {
            DataTable table = CreateTermTable();
            for (int i = Constants.TERM_ValidTermId; i < Constants.TERM_ValidTermId + count; i++)
            {
                string name = (count == 1) ? Constants.TERM_ValidName : ContentTestHelper.GetTermName(i);
                string description = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetTermName(i);
                int weight = Constants.TERM_ValidWeight;
                int parentId = Constants.TERM_ValidParentTermId;
                AddTermToTable(table, i, contentIdFunction(i), vocabularyIdFunction(i), name, description, weight, parentId);
            }

            return table.CreateDataReader();
        }

        internal static IDataReader CreateValidVocabulariesReader(int count)
        {
            DataTable table = CreateVocabularyTable();
            for (int i = Constants.VOCABULARY_ValidVocabularyId; i < Constants.VOCABULARY_ValidVocabularyId + count; i++)
            {
                string name = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetVocabularyName(i);
                int typeId = Constants.VOCABULARY_SimpleTypeId;
                string description = (count == 1) ? Constants.VOCABULARY_ValidName : ContentTestHelper.GetVocabularyName(i);
                int weight = Constants.VOCABULARY_ValidWeight;
                AddVocabularyToTable(table, i, typeId, name, description, Constants.VOCABULARY_ValidScopeId, Constants.VOCABULARY_ValidScopeTypeId, weight);
            }

            return table.CreateDataReader();
        }

        internal static IFileInfo CreateRandomFile(int fileId)
        {
            var files = Directory.EnumerateFiles(@"C:\").ToArray();

            var random = new Random((int)DateTime.UtcNow.Ticks);

            var fileInfo = new Mock<IFileInfo>();

            fileInfo.SetupGet(f => f.FileId).Returns(fileId);
            fileInfo.SetupGet(f => f.Extension).Returns(".txt");
            fileInfo.SetupGet(f => f.FileName).Returns(random.Next() + ".txt");
            fileInfo.SetupGet(f => f.Folder).Returns(@"\");
            fileInfo.SetupGet(f => f.FolderId).Returns(0);
            fileInfo.SetupGet(f => f.FolderMappingID).Returns(-1);
            fileInfo.SetupGet(f => f.IsCached).Returns(false);
            fileInfo.SetupGet(f => f.LastModificationTime).Returns(DateTime.UtcNow);
            fileInfo.SetupGet(f => f.PhysicalPath).Returns(() => files[random.Next() % (files.Length - 1)]);
            fileInfo.SetupGet(f => f.PortalId).Returns(0);
            fileInfo.SetupGet(f => f.RelativePath).Returns(@"..\..\");
            fileInfo.SetupGet(f => f.UniqueId).Returns(Guid.NewGuid());
            fileInfo.SetupGet(f => f.VersionGuid).Returns(Guid.NewGuid());
            fileInfo.SetupGet(f => f.Size).Returns(random.Next() % (1024 * 1024));

            return fileInfo.Object;
        }

        internal static Mock<IFileManager> CreateMockFileManager()
        {
            var fm = new Mock<IFileManager>();

            fm.Setup(f => f.GetFile(It.IsAny<int>())).Returns((int fileId) => CreateRandomFile(fileId));
            return fm;
        }

        private static void AddBaseEntityColumns(DataTable table)
        {
            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));
        }

        private static void AddContentItemToTable(DataTable table, int id, string content, string contentKey, bool indexed, int userId, string term)
        {
            table.Rows.Add(new object[] { id, content, Null.NullInteger, Null.NullInteger, Null.NullInteger, contentKey, indexed, userId, term });
        }

        private static void AddContentTypeToTable(DataTable table, int id, string contentType)
        {
            table.Rows.Add(new object[] { id, contentType });
        }

        private static void AddScopeTypeToTable(DataTable table, int id, string scopeType)
        {
            table.Rows.Add(new object[] { id, scopeType });
        }

        private static void AddTermToTable(DataTable table, int id, int contentItemId, int vocabularyId, string name, string description, int weight, int parentId)
        {
            table.Rows.Add(new object[] { id, contentItemId, vocabularyId, name, description, weight, parentId });
        }

        private static void AddVocabularyToTable(DataTable table, int id, int typeId, string name, string description, int scopeId, int scopeTypeId, int weight)
        {
            table.Rows.Add(new object[] { id, typeId, false, name, description, scopeId, scopeTypeId, weight });
        }

        private static DataTable CreateContentItemTable()
        {
            // Create Categories table.
            DataTable table = new DataTable();

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ContentItemID", typeof(int));
            table.Columns.Add("Content", typeof(string));
            table.Columns.Add("ContentTypeID", typeof(int));
            table.Columns.Add("TabID", typeof(int));
            table.Columns.Add("ModuleID", typeof(int));
            table.Columns.Add("ContentKey", typeof(string));
            table.Columns.Add("Indexed", typeof(bool));
            table.Columns.Add("UserID", typeof(int));
            table.Columns.Add("Term", typeof(string));
            table.Columns.Add("StateID", typeof(int));
            AddBaseEntityColumns(table);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };

            return table;
        }

        private static DataTable CreateContentTypeTable()
        {
            // Create ContentTypes table.
            DataTable table = new DataTable();

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ContentTypeID", typeof(int));
            table.Columns.Add("ContentType", typeof(string));

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };

            return table;
        }

        private static DataTable CreateScopeTypeTable()
        {
            // Create ScopeTypes table.
            DataTable table = new DataTable();

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("ScopeTypeID", typeof(int));
            table.Columns.Add("ScopeType", typeof(string));

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };

            return table;
        }

        private static DataTable CreateTermTable()
        {
            // Create Vocabulary table.
            DataTable table = new DataTable();

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("TermID", typeof(int));
            table.Columns.Add("ContentItemID", typeof(int));
            table.Columns.Add("VocabularyID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Weight", typeof(int));
            table.Columns.Add("ParentTermID", typeof(int));
            table.Columns.Add("TermLeft", typeof(int));
            table.Columns.Add("TermRight", typeof(int));
            AddBaseEntityColumns(table);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };

            return table;
        }

        private static DataTable CreateVocabularyTable()
        {
            // Create Vocabulary table.
            DataTable table = new DataTable();

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("VocabularyID", typeof(int));
            table.Columns.Add("VocabularyTypeID", typeof(int));
            table.Columns.Add("IsSystem", typeof(bool));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("ScopeID", typeof(int));
            table.Columns.Add("ScopeTypeID", typeof(int));
            table.Columns.Add("Weight", typeof(int));
            AddBaseEntityColumns(table);

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };

            return table;
        }

        private static Mock<TMock> RegisterMockController<TMock>(Mock<TMock> mock)
            where TMock : class
        {
            if (ComponentFactory.Container == null)
            {
                // Create a Container
                ComponentFactory.Container = new SimpleContainer();
            }

            // Try and get mock
            var getMock = ComponentFactory.GetComponent<Mock<TMock>>();

            if (getMock == null)
            {
                // Create the mock
                getMock = mock;

                // Add both mock and mock.Object to Container
                ComponentFactory.RegisterComponentInstance<Mock<TMock>>(getMock);
                ComponentFactory.RegisterComponentInstance<TMock>(getMock.Object);
            }

            return getMock;
        }
    }
}
