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

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Log.EventLog;

    public partial class PortalAliasController
    {
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
    }
}
