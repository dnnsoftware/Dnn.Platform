// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The business of mobile preview profiles.</summary>
    public class PreviewProfileController : IPreviewProfileController
    {
        private readonly IHostSettings hostSettings;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="PreviewProfileController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PreviewProfileController()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PreviewProfileController"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        public PreviewProfileController(IEventLogger eventLogger, IHostSettings hostSettings)
        {
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

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

            var logContent = $"{(profile.Id == Null.NullInteger ? "Add" : "Update")} Mobile Preview Profile '{profile.Name}'";
            AddLog(this.eventLogger, logContent);

            ClearCache(profile.PortalId);
        }

        /// <summary>delete a preview profile.</summary>
        /// <param name="portalId">Portal's id.</param>
        /// <param name="id">the profile's id.</param>
        public void Delete(int portalId, int id)
        {
            var delProfile = this.GetProfileById(portalId, id);
            if (delProfile != null)
            {
                // update the list order
                this.GetProfilesByPortal(portalId)
                    .Where(p => p.SortOrder > delProfile.SortOrder)
                    .ToList()
                    .ForEach(p =>
                    {
                        p.SortOrder--;
                        this.Save(p);
                    });
                DataProvider.Instance().DeletePreviewProfile(id);

                AddLog(this.eventLogger, string.Format(CultureInfo.InvariantCulture, "Delete Mobile Preview Profile '{0}'", id));

                ClearCache(portalId);
            }
        }

        /// <summary>get a preview profiles list for portal.</summary>
        /// <param name="portalId">portal id.</param>
        /// <returns>List of preview profile.</returns>
        public IList<IPreviewProfile> GetProfilesByPortal(int portalId)
        {
            return this.GetProfilesByPortal(portalId, true);
        }

        /// <summary>get a specific preview profile by id.</summary>
        /// <param name="portalId">the ID of the portal to which the profile belongs.</param>
        /// <param name="id">profile's id.</param>
        /// <returns>profile object.</returns>
        public IPreviewProfile GetProfileById(int portalId, int id)
        {
            return this.GetProfilesByPortal(portalId).Where(r => r.Id == id).FirstOrDefault();
        }

        private static void ClearCache(int portalId)
        {
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.PreviewProfilesCacheKey, portalId));
        }

        private static void AddLog(IEventLogger eventLogger, string logContent)
        {
            eventLogger.AddLog("Message", logContent, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogType.ADMIN_ALERT);
        }

        private IList<IPreviewProfile> GetProfilesByPortal(int portalId, bool addDefault)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PreviewProfilesCacheKey, portalId);
            var cacheArg = new CacheItemArgs(cacheKey, DataCache.PreviewProfilesCacheTimeOut, DataCache.PreviewProfilesCachePriority, portalId, addDefault);
            return CBO.GetCachedObject<IList<IPreviewProfile>>(this.hostSettings, cacheArg, this.GetProfilesByPortalIdCallBack);
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

        private List<PreviewProfile> CreateDefaultDevices(int portalId)
        {
            var settings = PortalController.Instance.GetPortalSettings(portalId);
            List<PreviewProfile> profiles = [];

            if (!settings.TryGetValue("DefPreviewProfiles_Created", out var defaultPreviewProfiles) || defaultPreviewProfiles != DotNetNukeContext.Current.Application.Name)
            {
                try
                {
                    var defaultDeviceDBPath = Config.GetSetting("DefaultDevicesDatabase");
                    if (!string.IsNullOrEmpty(defaultDeviceDBPath))
                    {
                        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultDeviceDBPath);

                        if (!string.IsNullOrEmpty(dataPath) && File.Exists(dataPath))
                        {
                            var serializer = new XmlSerializer(typeof(List<PreviewProfile>));
                            using (var fileStream = File.OpenRead(dataPath))
                            using (var xmlReader = XmlReader.Create(fileStream))
                            {
                                profiles = (List<PreviewProfile>)serializer.Deserialize(xmlReader);
                            }

                            profiles?.ForEach(p =>
                            {
                                p.PortalId = portalId;
                                this.Save(p);
                            });
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
