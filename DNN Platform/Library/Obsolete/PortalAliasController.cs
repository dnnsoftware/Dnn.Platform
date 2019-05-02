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

using System.ComponentModel;

#region Usings

using System;
using System.Collections;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;

#endregion

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Entities.Portals
{
    public partial class PortalAliasController
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in version 7.1.  Replaced by PortalAliasController.Instance.DeletePortalAlias. Scheduled removal in v10.0.0.")]
        public void DeletePortalAlias(int portalAliasId)
        {

            DataProvider.Instance().DeletePortalAlias(portalAliasId);

            EventLogController.Instance.AddLog("PortalAliasID",
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
        [Obsolete("Deprecated in version 7.1.  Replaced by PortalAliasController.Instance.UpdatePortalAlias. Scheduled removal in v10.0.0.")]
        public void UpdatePortalAliasInfo(PortalAliasInfo portalAlias)
        {
            Instance.UpdatePortalAlias(portalAlias);
        }
    }
}
