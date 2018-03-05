using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using DotNetNuke.Common.Utilities;
using DNN.Integration.Test.Framework;
using DNN.Integration.Test.Framework.Helpers;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Modules.DigitalAssets
{
    [TestFixture]
    public class DigitalAssetsTests : IntegrationTestBase
    {
        #region Fields

        private readonly int PortalId = 0;

        #endregion

        #region SetUp

        public DigitalAssetsTests()
        {
        }

        #endregion

        #region Tests

        [Test]
        public void File_Url_Should_Update_After_Rename_Folder()
        {
            var connector = WebApiTestHelper.LoginAdministrator();

            var folder = CreateNewFolder(connector);
            var folderId = Convert.ToInt32(folder.FolderID);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files\\Test.png");
            connector.UploadCmsFile(filePath, folder.FolderPath.ToString());
            var fileId = GetFileId(folderId, "Test.png");

            var newFolderName = Guid.NewGuid().ToString();
            RenameFolder(connector, folderId, newFolderName);

            var getUrlApi = "API/DigitalAssets/ContentService/GetUrl";
            var fileUrl = connector.PostJson(getUrlApi, new {fileId = fileId}, GetRequestHeaders()).Content.ReadAsStringAsync().Result;

            Assert.IsTrue(fileUrl.Contains(newFolderName));
        }

        #endregion

        #region Private Methods

        private int GetRootFolderId()
        {
            return DatabaseHelper.ExecuteScalar<int>(
                $"SELECT FolderID FROM {{objectQualifier}}[Folders] WHERE PortalId = {PortalId} AND FolderPath = ''");
        }

        private int GetStandardFolderMappingId()
        {
            return DatabaseHelper.ExecuteScalar<int>(
                $"SELECT FolderMappingId from {{objectQualifier}}[FolderMappings] where PortalID = {PortalId} and MappingName = 'Standard'");
        }

        private int GetFileId(int folderId, string fileName)
        {
            return DatabaseHelper.ExecuteScalar<int>(
                $"select FileId from {{objectQualifier}}[Files] WHERE FolderID = {folderId} and FileName = '{fileName}'");
        }

        private dynamic CreateNewFolder(IWebApiConnector connector)
        {
            var rootFolderId = GetRootFolderId();
            var apiUrl = "API/DigitalAssets/ContentService/CreateNewFolder";
            var postData = new
            {
                FolderName = Guid.NewGuid().ToString(),
                ParentFolderId = rootFolderId,
                FolderMappingId = GetStandardFolderMappingId(),
                MappedName = string.Empty
            };

            var response = connector.PostJson(apiUrl, postData, GetRequestHeaders());
            return Json.Deserialize<dynamic>(response.Content.ReadAsStringAsync().Result);
        }

        private void RenameFolder(IWebApiConnector connector, int folderId, string newFolderName)
        {
            var apiUrl = "API/DigitalAssets/ContentService/RenameFolder";
            var postData = new
            {
                folderId = folderId,
                newFolderName = newFolderName
            };

            connector.PostJson(apiUrl, postData, GetRequestHeaders());
        }

        private IDictionary<string, string> GetRequestHeaders()
        {
            return WebApiTestHelper.GetRequestHeaders("//Admin//FileManagement", "Digital Asset Management", PortalId);
        }

        #endregion
    }
}
