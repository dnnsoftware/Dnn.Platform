#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Data;

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Services.Mobile;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Services.Mobile
{
	/// <summary>
	///   Summary description for PreviewProfileControllerTests
	/// </summary>
	[TestFixture]
	public class PreviewProfileControllerTests
	{
		#region "Private Properties"

		private Mock<DataProvider> _dataProvider;

		private DataTable _dtProfiles;

		#endregion

		#region "Set Up"

		[SetUp]
		public void SetUp()
		{
			ComponentFactory.Container = new SimpleContainer();
			_dataProvider = MockComponentProvider.CreateDataProvider();
			_dataProvider.Setup(d => d.GetProviderPath()).Returns("");
			MockComponentProvider.CreateDataCacheProvider();
			MockComponentProvider.CreateEventLogController();

			_dtProfiles = new DataTable("PreviewProfiles");
			var pkCol = _dtProfiles.Columns.Add("Id", typeof(int));
			_dtProfiles.Columns.Add("PortalId", typeof(int));
			_dtProfiles.Columns.Add("Name", typeof(string));
			_dtProfiles.Columns.Add("Width", typeof(int));
			_dtProfiles.Columns.Add("Height", typeof(int));
			_dtProfiles.Columns.Add("UserAgent", typeof(string));
			_dtProfiles.Columns.Add("SortOrder", typeof(int));

			_dtProfiles.PrimaryKey = new[] { pkCol };

			_dataProvider.Setup(d =>
								d.SavePreviewProfile(It.IsAny<int>(),
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
																	if (_dtProfiles.Rows.Count == 0)
																	{
																		id = 1;
																	}
																	else
																	{
																		id = Convert.ToInt32(_dtProfiles.Select("", "Id Desc")[0]["Id"]) + 1;
																	}

                                                                    var row = _dtProfiles.NewRow();
																	row["Id"] = id;
																	row["PortalId"] = portalId;
																	row["name"] = name;
																	row["width"] = width;
																	row["height"] = height;
																	row["useragent"] = userAgent;
																	row["sortorder"] = sortOrder;

																	_dtProfiles.Rows.Add(row);
																}
																else
																{
																	var rows = _dtProfiles.Select("Id = " + id);
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

			_dataProvider.Setup(d => d.GetPreviewProfiles(It.IsAny<int>())).Returns<int>((portalId) => { return GetProfilesCallBack(portalId); });
			_dataProvider.Setup(d => d.DeletePreviewProfile(It.IsAny<int>())).Callback<int>((id) =>
																							{
																								var rows = _dtProfiles.Select("Id = " + id);
																								if (rows.Length == 1)
																								{
																									_dtProfiles.Rows.Remove(rows[0]);
																								}
																							});
		}

		#endregion

		#region "Test"

		[Test]
		public void PreviewProfileController_Save_Valid_Profile()
		{
			var profile = new PreviewProfile { Name = "Test R", PortalId = 0, Width = 800, Height = 480 };
			new PreviewProfileController().Save(profile);

			var dataReader = _dataProvider.Object.GetPreviewProfiles(0);
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
			PrepareData();

			IList<IPreviewProfile> list = new PreviewProfileController().GetProfilesByPortal(0);

			Assert.AreEqual(3, list.Count);
		}

		[Test]
		public void PreviewProfileController_Delete_With_ValidID()
		{
			PrepareData();
			new PreviewProfileController().Delete(0, 1);

			IList<IPreviewProfile> list = new PreviewProfileController().GetProfilesByPortal(0);

			Assert.AreEqual(2, list.Count);
		}

		#endregion

		#region "Private Methods"

		private IDataReader GetProfilesCallBack(int portalId)
		{
			var dtCheck = _dtProfiles.Clone();
			foreach (var row in _dtProfiles.Select("PortalId = " + portalId))
			{
				dtCheck.Rows.Add(row.ItemArray);
			}

			return dtCheck.CreateDataReader();
		}

		private void PrepareData()
		{
			_dtProfiles.Rows.Add(1, 0, "R1", 640, 480, "", 1);
			_dtProfiles.Rows.Add(2, 0, "R2", 640, 480, "", 2);
			_dtProfiles.Rows.Add(3, 0, "R3", 640, 480, "", 3);
			_dtProfiles.Rows.Add(4, 1, "R4", 640, 480, "", 4);
			_dtProfiles.Rows.Add(5, 1, "R5", 640, 480, "", 5);
			_dtProfiles.Rows.Add(6, 1, "R6", 640, 480, "", 6);
		}

		#endregion
	}
}