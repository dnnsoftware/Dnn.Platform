// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
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

    /// <summary>
    /// The business of mobile preview profiles.
    /// </summary>
    public class PreviewProfileController : IPreviewProfileController
    {
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
                profile.SortOrder = this.GetProfilesByPortal(profile.PortalId, false).Count + 1;
            }

            int id = DataProvider.Instance().SavePreviewProfile(
                profile.Id,
                profile.PortalId,
                profile.Name,
                profile.Width,
                profile.Height,
                profile.UserAgent,
                profile.SortOrder,
                UserController.Instance.GetCurrentUserInfo().UserID);

            profile.Id = id;

            var logContent = string.Format("{0} Mobile Preview Profile '{1}'", profile.Id == Null.NullInteger ? "Add" : "Update", profile.Name);
            this.AddLog(logContent);

            this.ClearCache(profile.PortalId);
        }

        /// <summary>
        /// delete a preview profile.
        /// </summary>
        /// <param name="portalId">Portal's id.</param>
        /// <param name="id">the profile's id.</param>
        public void Delete(int portalId, int id)
        {
            var delProfile = this.GetProfileById(portalId, id);
            if (delProfile != null)
            {
                // update the list order
                this.GetProfilesByPortal(portalId).Where(p => p.SortOrder > delProfile.SortOrder).ToList().ForEach(p =>
                                                                                                                {
                                                                                                                    p.SortOrder--;
                                                                                                                    this.Save(p);
                                                                                                                });
                DataProvider.Instance().DeletePreviewProfile(id);

                var logContent = string.Format("Delete Mobile Preview Profile '{0}'", id);
                this.AddLog(logContent);

                this.ClearCache(portalId);
            }
        }

        /// <summary>
        /// get a preview profiles list for portal.
        /// </summary>
        /// <param name="portalId">portal id.</param>
        /// <returns>List of preview profile.</returns>
        public IList<IPreviewProfile> GetProfilesByPortal(int portalId)
        {
            return this.GetProfilesByPortal(portalId, true);
        }

        /// <summary>
        /// get a specific preview profile by id.
        /// </summary>
        /// <param name="portalId">the profile belong's portal.</param>
        /// <param name="id">profile's id.</param>
        /// <returns>profile object.</returns>
        public IPreviewProfile GetProfileById(int portalId, int id)
        {
            return this.GetProfilesByPortal(portalId).Where(r => r.Id == id).FirstOrDefault();
        }

        private IList<IPreviewProfile> GetProfilesByPortal(int portalId, bool addDefault)
        {
            string cacheKey = string.Format(DataCache.PreviewProfilesCacheKey, portalId);
            var cacheArg = new CacheItemArgs(cacheKey, DataCache.PreviewProfilesCacheTimeOut, DataCache.PreviewProfilesCachePriority, portalId, addDefault);
            return CBO.GetCachedObject<IList<IPreviewProfile>>(cacheArg, this.GetProfilesByPortalIdCallBack);
        }

        private IList<IPreviewProfile> GetProfilesByPortalIdCallBack(CacheItemArgs cacheItemArgs)
        {
            int portalId = (int)cacheItemArgs.ParamList[0];
            bool addDefault = (bool)cacheItemArgs.ParamList[1];

            var profiles = CBO.FillCollection<PreviewProfile>(DataProvider.Instance().GetPreviewProfiles(portalId));
            if (profiles.Count == 0 && addDefault)
            {
                profiles = this.CreateDefaultDevices(portalId);
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
                            var serializer = new XmlSerializer(typeof(List<PreviewProfile>));
                            profiles = (List<PreviewProfile>)serializer.Deserialize(File.OpenRead(dataPath));

                            if (profiles != null)
                            {
                                profiles.ForEach(p =>
                                                     {
                                                         p.PortalId = portalId;

                                                         this.Save(p);
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
    }
}
