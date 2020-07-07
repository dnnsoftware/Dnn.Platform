// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Mobile
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Services.Mobile;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    ///   Summary description for PreviewProfileControllerTests.
    /// </summary>
    [TestFixture]
    public class PreviewProfileControllerTests
    {
        private Mock<DataProvider> _dataProvider;

        private DataTable _dtProfiles;

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            this._dataProvider = MockComponentProvider.CreateDataProvider();
            this._dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
            MockComponentProvider.CreateDataCacheProvider();
            MockComponentProvider.CreateEventLogController();

            this._dtProfiles = new DataTable("PreviewProfiles");
            var pkCol = this._dtProfiles.Columns.Add("Id", typeof(int));
            this._dtProfiles.Columns.Add("PortalId", typeof(int));
            this._dtProfiles.Columns.Add("Name", typeof(string));
            this._dtProfiles.Columns.Add("Width", typeof(int));
            this._dtProfiles.Columns.Add("Height", typeof(int));
            this._dtProfiles.Columns.Add("UserAgent", typeof(string));
            this._dtProfiles.Columns.Add("SortOrder", typeof(int));

            this._dtProfiles.PrimaryKey = new[] { pkCol };

            this._dataProvider.Setup(d =>
                                d.SavePreviewProfile(
                                    It.IsAny<int>(),
                                    It.IsAny<int>(),
                                    It.IsAny<string>(),
                                    It.IsAny<int>(),
                                    It.IsAny<int>(),
                                    It.IsAny<string>(),
                                    It.IsAny<int>(),
                                    It.IsAny<int>())).Returns<int, int, string, int, int, string, int, int>(
                                                            (id, portalId, name, width, height, userAgent, sortOrder, userId) =>
                                                            {
                                                                if (id == -1)
                                                                {
                                                                    if (this._dtProfiles.Rows.Count == 0)
                                                                    {
                                                                        id = 1;
                                                                    }
                                                                    else
                                                                    {
                                                                        id = Convert.ToInt32(this._dtProfiles.Select(string.Empty, "Id Desc")[0]["Id"]) + 1;
                                                                    }

                                                                    var row = this._dtProfiles.NewRow();
                                                                    row["Id"] = id;
                                                                    row["PortalId"] = portalId;
                                                                    row["name"] = name;
                                                                    row["width"] = width;
                                                                    row["height"] = height;
                                                                    row["useragent"] = userAgent;
                                                                    row["sortorder"] = sortOrder;

                                                                    this._dtProfiles.Rows.Add(row);
                                                                }
                                                                else
                                                                {
                                                                    var rows = this._dtProfiles.Select("Id = " + id);
                                                                    if (rows.Length == 1)
                                                                    {
                                                                        var row = rows[0];

                                                                        row["name"] = name;
                                                                        row["width"] = width;
                                                                        row["height"] = height;
                                                                        row["useragent"] = userAgent;
                                                                        row["sortorder"] = sortOrder;
                                                                    }
                                                                }

                                                                return id;
                                                            });

            this._dataProvider.Setup(d => d.GetPreviewProfiles(It.IsAny<int>())).Returns<int>((portalId) => { return this.GetProfilesCallBack(portalId); });
            this._dataProvider.Setup(d => d.DeletePreviewProfile(It.IsAny<int>())).Callback<int>((id) =>
                                                                                            {
                                                                                                var rows = this._dtProfiles.Select("Id = " + id);
                                                                                                if (rows.Length == 1)
                                                                                                {
                                                                                                    this._dtProfiles.Rows.Remove(rows[0]);
                                                                                                }
                                                                                            });
        }

        [Test]
        public void PreviewProfileController_Save_Valid_Profile()
        {
            var profile = new PreviewProfile { Name = "Test R", PortalId = 0, Width = 800, Height = 480 };
            new PreviewProfileController().Save(profile);

            var dataReader = this._dataProvider.Object.GetPreviewProfiles(0);
            var affectedCount = 0;
            while (dataReader.Read())
            {
                affectedCount++;
            }

            Assert.AreEqual(1, affectedCount);
        }

        [Test]
        public void PreviewProfileController_GetProfilesByPortal_With_Valid_PortalID()
        {
            this.PrepareData();

            IList<IPreviewProfile> list = new PreviewProfileController().GetProfilesByPortal(0);

            Assert.AreEqual(3, list.Count);
        }

        [Test]
        public void PreviewProfileController_Delete_With_ValidID()
        {
            this.PrepareData();
            new PreviewProfileController().Delete(0, 1);

            IList<IPreviewProfile> list = new PreviewProfileController().GetProfilesByPortal(0);

            Assert.AreEqual(2, list.Count);
        }

        private IDataReader GetProfilesCallBack(int portalId)
        {
            var dtCheck = this._dtProfiles.Clone();
            foreach (var row in this._dtProfiles.Select("PortalId = " + portalId))
            {
                dtCheck.Rows.Add(row.ItemArray);
            }

            return dtCheck.CreateDataReader();
        }

        private void PrepareData()
        {
            this._dtProfiles.Rows.Add(1, 0, "R1", 640, 480, string.Empty, 1);
            this._dtProfiles.Rows.Add(2, 0, "R2", 640, 480, string.Empty, 2);
            this._dtProfiles.Rows.Add(3, 0, "R3", 640, 480, string.Empty, 3);
            this._dtProfiles.Rows.Add(4, 1, "R4", 640, 480, string.Empty, 4);
            this._dtProfiles.Rows.Add(5, 1, "R5", 640, 480, string.Empty, 5);
            this._dtProfiles.Rows.Add(6, 1, "R6", 640, 480, string.Empty, 6);
        }
    }
}
