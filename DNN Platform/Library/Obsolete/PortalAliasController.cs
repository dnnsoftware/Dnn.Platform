// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Linq;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Log.EventLog;
    using Microsoft.Extensions.DependencyInjection;

    public partial class PortalAliasController : ServiceLocator<IPortalAliasController, PortalAliasController>, IPortalAliasController
    {
        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use DotNetNuke.Abstractions.Portals.IPortalAliasSettingsService instead.")]
        public static new IPortalAliasController Instance
        {
            get
            {
                var portalAliasSettingsService = Globals.DependencyProvider.GetRequiredService<IPortalAliasService>();
                return portalAliasSettingsService is IPortalAliasController castedController ? castedController : new PortalAliasController();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in version 7.3.  Replaced by PortalAliasController.Instance.GetPortalAlias. Scheduled removal in v10.0.0.")]
        public static PortalAliasInfo GetPortalAliasInfo(string httpAlias)
        {
            return Instance.GetPortalAlias(httpAlias);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in version 7.1.  Replaced by PortalAliasController.Instance.GetPortalAliases. Scheduled removal in v10.0.0.")]
        public static PortalAliasCollection GetPortalAliasLookup()
        {
            var portalAliasCollection = new PortalAliasCollection();
            var aliasController = new PortalAliasController();
            foreach (var kvp in aliasController.GetPortalAliasesInternal())
            {
                portalAliasCollection.Add(kvp.Key, kvp.Value);
            }

            return portalAliasCollection;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in version 7.3.  Replaced by PortalAliasController.Instance.GetPortalAlias. Scheduled removal in v10.0.0.")]
        public static PortalAliasInfo GetPortalAliasLookup(string httpAlias)
        {
            return Instance.GetPortalAlias(httpAlias);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in version 7.1.  Replaced by PortalAliasController.Instance.DeletePortalAlias. Scheduled removal in v10.0.0.")]
        public void DeletePortalAlias(int portalAliasId)
        {
            DataProvider.Instance().DeletePortalAlias(portalAliasId);

            EventLogController.Instance.AddLog(
                "PortalAliasID",
                portalAliasId.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.PORTALALIAS_DELETED);

            DataCache.RemoveCache(DataCache.PortalAliasCacheKey);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in version 7.1.  Replaced by PortalAliasController.Instance.GetPortalAliasesByPortalId. Scheduled removal in v10.0.0.")]
        public ArrayList GetPortalAliasArrayByPortalID(int PortalID)
        {
            return new ArrayList(Instance.GetPortalAliasesByPortalId(PortalID).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in version 7.1.  Replaced by PortalAliasController.Instance.GetPortalAliasesByPortalId. Scheduled removal in v10.0.0.")]
        public PortalAliasCollection GetPortalAliasByPortalID(int PortalID)
        {
            var portalAliasCollection = new PortalAliasCollection();

            foreach (PortalAliasInfo alias in GetPortalAliasLookup().Values.Cast<PortalAliasInfo>().Where(alias => alias.PortalID == PortalID))
            {
                portalAliasCollection.Add(alias.HTTPAlias, alias);
            }

            return portalAliasCollection;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in version 7.1.  Replaced by PortalAliasController.Instance.UpdatePortalAlias. Scheduled removal in v10.0.0.")]
        public void UpdatePortalAliasInfo(PortalAliasInfo portalAlias)
        {
            Instance.UpdatePortalAlias(portalAlias);
        }

        /// <summary>
        /// Gets the portal alias by portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use DotNetNuke.Abstractions.Portals.IPortalAliasSettingsService instead.")]
        public static string GetPortalAliasByPortal(int portalId, string portalAlias) =>
            ((IPortalAliasService)Instance).GetPortalAliasByPortal(portalId, portalAlias);

        /// <summary>
        /// Gets the portal alias by tab.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use DotNetNuke.Abstractions.Portals.IPortalAliasSettingsService instead.")]
        public static string GetPortalAliasByTab(int tabId, string portalAlias) =>
            ((IPortalAliasService)Instance).GetPortalAliasByTab(tabId, portalAlias);

        /// <summary>
        /// Validates the alias.
        /// </summary>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="ischild">if set to <c>true</c> [ischild].</param>
        /// <returns><c>true</c> if the alias is a valid url format; otherwise return <c>false</c>.</returns>
        [Obsolete("Deprecated in 9.7.2. Scheduled for removal in v11.0.0, use DotNetNuke.Abstractions.Portals.IPortalAliasSettingsService instead.")]
        public static bool ValidateAlias(string portalAlias, bool ischild) =>
            ((IPortalAliasService)Instance).ValidateAlias(portalAlias, ischild);
    }
}
