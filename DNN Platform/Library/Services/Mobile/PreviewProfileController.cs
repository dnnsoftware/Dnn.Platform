﻿#region Copyright
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
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Services.Mobile
{
	/// <summary>
	/// The business of mobile preview profiles.
	/// </summary>
	public class PreviewProfileController : IPreviewProfileController
	{
		#region "Public Methods"
		/// <summary>
		/// save a preview profile. If profile.Id equals Null.NullInteger(-1), that means need to add a new profile;
		/// otherwise will update the profile by profile.Id.
		/// </summary>
		/// <param name="profile">profile object.</param>
		public void Save(IPreviewProfile profile)
		{
			Requires.NotNull("The profile can't be null", profile);

			if (profile.Id == Null.NullInteger || profile.SortOrder == 0)
			{
				profile.SortOrder = GetProfilesByPortal(profile.PortalId, false).Count + 1;
			}

			int id = DataProvider.Instance().SavePreviewProfile(profile.Id,
														profile.PortalId,
														profile.Name,
														profile.Width,
														profile.Height,
														profile.UserAgent,
														profile.SortOrder,
														UserController.Instance.GetCurrentUserInfo().UserID);

			profile.Id = id;

			var logContent = string.Format("{0} Mobile Preview Profile '{1}'", profile.Id == Null.NullInteger ? "Add" : "Update", profile.Name);
			AddLog(logContent);

			ClearCache(profile.PortalId);
		}

		/// <summary>
		/// delete a preview profile.
		/// </summary>
		/// <param name="portalId">Portal's id.</param>
		/// <param name="id">the profile's id.</param>
		public void Delete(int portalId, int id)
		{
			var delProfile = GetProfileById(portalId, id);
			if (delProfile != null)
			{
				//update the list order
				GetProfilesByPortal(portalId).Where(p => p.SortOrder > delProfile.SortOrder).ToList().ForEach(p =>
																												{
																													p.SortOrder--;
																													Save(p);
																												});
				DataProvider.Instance().DeletePreviewProfile(id);

				var logContent = string.Format("Delete Mobile Preview Profile '{0}'", id);
				AddLog(logContent);

				ClearCache(portalId);
			}
		}

		/// <summary>
		/// get a preview profiles list for portal.
		/// </summary>
		/// <param name="portalId">portal id.</param>
		/// <returns>List of preview profile.</returns>
		public IList<IPreviewProfile> GetProfilesByPortal(int portalId)
		{
			return GetProfilesByPortal(portalId, true);
		}

		/// <summary>
		/// get a specific preview profile by id.
		/// </summary>
		/// <param name="portalId">the profile belong's portal.</param>
		/// <param name="id">profile's id.</param>
		/// <returns>profile object.</returns>
		public IPreviewProfile GetProfileById(int portalId, int id)
		{
			return GetProfilesByPortal(portalId).Where(r => r.Id == id).FirstOrDefault();
		}

		#endregion

		#region "Private Methods"

		private IList<IPreviewProfile> GetProfilesByPortal(int portalId, bool addDefault)
		{
			string cacheKey = string.Format(DataCache.PreviewProfilesCacheKey, portalId);
			var cacheArg = new CacheItemArgs(cacheKey, DataCache.PreviewProfilesCacheTimeOut, DataCache.PreviewProfilesCachePriority, portalId, addDefault);
			return CBO.GetCachedObject<IList<IPreviewProfile>>(cacheArg, GetProfilesByPortalIdCallBack);
		}

		private IList<IPreviewProfile> GetProfilesByPortalIdCallBack(CacheItemArgs cacheItemArgs)
		{
			int portalId = (int)cacheItemArgs.ParamList[0];
			bool addDefault = (bool)cacheItemArgs.ParamList[1];

			var profiles = CBO.FillCollection<PreviewProfile>(DataProvider.Instance().GetPreviewProfiles(portalId));
			if (profiles.Count == 0 && addDefault)
			{
				profiles = CreateDefaultDevices(portalId);
			}

			return profiles.Cast<IPreviewProfile>().ToList();
		}

		private void ClearCache(int portalId)
		{
			DataCache.RemoveCache(string.Format(DataCache.PreviewProfilesCacheKey, portalId));
		}

		private void AddLog(string logContent)
		{
            EventLogController.Instance.AddLog("Message", logContent, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.ADMIN_ALERT);
		}


		private List<PreviewProfile> CreateDefaultDevices(int portalId)
		{
			string defaultPreviewProfiles;
            var settings = PortalController.Instance.GetPortalSettings(portalId);
			List<PreviewProfile> profiles = new List<PreviewProfile>();

			if (!settings.TryGetValue("DefPreviewProfiles_Created", out defaultPreviewProfiles) || defaultPreviewProfiles != DotNetNukeContext.Current.Application.Name)
			{
				try
				{
				    var defaultDeviceDBPath = Config.GetSetting("DefaultDevicesDatabase");
                    if (!string.IsNullOrEmpty(defaultDeviceDBPath))
                    {
                        var dataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, defaultDeviceDBPath);

                        if (!string.IsNullOrEmpty(dataPath) && File.Exists(dataPath))
                        {
                            var serializer = new XmlSerializer(typeof (List<PreviewProfile>));
                            profiles = (List<PreviewProfile>) serializer.Deserialize(File.OpenRead(dataPath));

                            if (profiles != null)
                            {
                                profiles.ForEach(p =>
                                                     {
                                                         p.PortalId = portalId;

                                                         Save(p);
                                                     });
                            }
                        }
                    }

					PortalController.UpdatePortalSetting(portalId, "DefPreviewProfiles_Created", DotNetNukeContext.Current.Application.Name);
				}
				catch (Exception ex)
				{
					Exceptions.Exceptions.LogException(ex);
				}
			}

			return profiles;
		}
		#endregion
	}
}
