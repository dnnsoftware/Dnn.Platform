// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>PortalAliasController provides method to manage portal alias.</summary>
    /// <remarks>
    /// For DotNetNuke to know what site a request should load, it uses a system of portal aliases.
    /// When a request is received by DotNetNuke from IIS, it extracts the domain name portion and does a comparison against
    /// the list of portal aliases and then redirects to the relevant portal to load the appropriate page.
    /// </remarks>
    public partial class PortalAliasController : IPortalAliasService
    {
        private IPortalAliasService ThisAsInterface => this;

        /// <inheritdoc/>
        string IPortalAliasService.GetPortalAliasByPortal(int portalId, string portalAlias)
        {
            string retValue = string.Empty;
            bool foundAlias = false;
            var portalAliasInfo = this.ThisAsInterface.GetPortalAlias(portalAlias, portalId);
            if (portalAliasInfo != null)
            {
                retValue = portalAliasInfo.HttpAlias;
                foundAlias = true;
            }

            if (!foundAlias)
            {
                // searching from longest to shortest alias ensures that the most specific portal is matched first
                // In some cases this method has been called with "portalaliases" that were not exactly the real portal alias
                // the startswith behaviour is preserved here to support those non-specific uses
                var controller = new PortalAliasController();
                var portalAliases = controller.GetPortalAliasesInternal();
                var portalAliasCollection = portalAliases.OrderByDescending(k => k.Key.Length);

                foreach (var currentAlias in portalAliasCollection)
                {
                    // check if the alias key starts with the portal alias value passed in - we use
                    // StartsWith because child portals are redirected to the parent portal domain name
                    // eg. child = 'www.domain.com/child' and parent is 'www.domain.com'
                    // this allows the parent domain name to resolve to the child alias ( the tabid still identifies the child portalid )
                    IPortalAliasInfo currentAliasInfo = currentAlias.Value;
                    string httpAlias = currentAliasInfo.HttpAlias.ToLowerInvariant();
                    if (httpAlias.StartsWith(portalAlias.ToLowerInvariant()) && currentAliasInfo.PortalId == portalId)
                    {
                        retValue = currentAliasInfo.HttpAlias;
                        break;
                    }

                    httpAlias = httpAlias.StartsWith("www.") ? httpAlias.Replace("www.", string.Empty) : string.Concat("www.", httpAlias);
                    if (httpAlias.StartsWith(portalAlias.ToLowerInvariant()) && currentAliasInfo.PortalId == portalId)
                    {
                        retValue = currentAliasInfo.HttpAlias;
                        break;
                    }
                }
            }

            return retValue;
        }

        /// <inheritdoc/>
        string IPortalAliasService.GetPortalAliasByTab(int tabId, string portalAlias)
        {
            string retValue = Null.NullString;
            int intPortalId = -2;

            // get the tab
            TabInfo tab = TabController.Instance.GetTab(tabId, Null.NullInteger);
            if (tab != null)
            {
                // ignore deleted tabs
                if (!tab.IsDeleted)
                {
                    intPortalId = tab.PortalID;
                }
            }

            switch (intPortalId)
            {
                case -2: // tab does not exist
                    break;
                case -1: // host tab
                    // host tabs are not verified to determine if they belong to the portal alias
                    retValue = portalAlias;
                    break;
                default: // portal tab
                    retValue = this.ThisAsInterface.GetPortalAliasByPortal(intPortalId, portalAlias);
                    break;
            }

            return retValue;
        }

        /// <inheritdoc />
        bool IPortalAliasService.ValidateAlias(string portalAlias, bool isChild)
        {
            if (isChild)
            {
                return ValidateAlias(portalAlias, true, false);
            }

            // validate the domain
            Uri result;
            if (Uri.TryCreate(Globals.AddHTTP(portalAlias), UriKind.Absolute, out result))
            {
                return ValidateAlias(result.Host, false, true) && ValidateAlias(portalAlias, false, false);
            }

            return false;
        }

        /// <inheritdoc />
        int IPortalAliasService.AddPortalAlias(IPortalAliasInfo portalAlias)
        {
            // Add Alias
            var dataProvider = DataProvider.Instance();
            int id = dataProvider.AddPortalAlias(
                portalAlias.PortalId,
                portalAlias.HttpAlias.ToLowerInvariant().Trim('/'),
                portalAlias.CultureCode,
                portalAlias.Skin,
                portalAlias.BrowserType.ToString(),
                portalAlias.IsPrimary,
                UserController.Instance.GetCurrentUserInfo().UserID);

            // Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_CREATED);

            // clear portal alias cache
            ClearCache(true);

            return id;
        }

        /// <inheritdoc />
        void IPortalAliasService.DeletePortalAlias(IPortalAliasInfo portalAlias)
        {
            // Delete Alias
            DataProvider.Instance().DeletePortalAlias(portalAlias.PortalAliasId);

            // Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_DELETED);

            // clear portal alias cache
            ClearCache(false, portalAlias.PortalId);
        }

        /// <inheritdoc />
        IPortalAliasInfo IPortalAliasService.GetPortalAlias(string alias) =>
            this.GetPortalAliasInternal(alias);

        /// <inheritdoc />
        IPortalAliasInfo IPortalAliasService.GetPortalAlias(string alias, int portalId) =>
            this.GetPortalAliasesInternal()
                .SingleOrDefault((portalAliasInfo) => portalAliasInfo.Key.Equals(alias, StringComparison.InvariantCultureIgnoreCase) && ((IPortalAliasInfo)portalAliasInfo.Value).PortalId == portalId).Value;

        /// <inheritdoc />
        IPortalAliasInfo IPortalAliasService.GetPortalAliasByPortalAliasId(int portalAliasId) =>
            this.GetPortalAliasesInternal()
                .Values
                .Cast<IPortalAliasInfo>()
                .SingleOrDefault(portalAliasInfo => portalAliasInfo.PortalAliasId == portalAliasId);

        /// <inheritdoc />
        IDictionary<string, IPortalAliasInfo> IPortalAliasService.GetPortalAliases()
        {
            var aliasCollection = new Dictionary<string, IPortalAliasInfo>();
            foreach (IPortalAliasInfo alias in this.GetPortalAliasesInternal().Values)
            {
                aliasCollection.Add(alias.HttpAlias, alias);
            }

            return aliasCollection;
        }

        /// <inheritdoc />
        IEnumerable<IPortalAliasInfo> IPortalAliasService.GetPortalAliasesByPortalId(int portalId) =>
            this.GetPortalAliasesInternal().Values.Cast<IPortalAliasInfo>().Where(alias => alias.PortalId == portalId).ToList();

        /// <inheritdoc />
        IPortalInfo IPortalAliasService.GetPortalByPortalAliasId(int portalAliasId) =>
            CBO.FillObject<PortalInfo>(DataProvider.Instance().GetPortalByPortalAliasID(portalAliasId));

        /// <inheritdoc />
        void IPortalAliasService.UpdatePortalAlias(IPortalAliasInfo portalAlias)
        {
            // Update Alias
            DataProvider.Instance().UpdatePortalAliasInfo(
                portalAlias.PortalAliasId,
                portalAlias.PortalId,
                portalAlias.HttpAlias.ToLowerInvariant().Trim('/'),
                portalAlias.CultureCode,
                portalAlias.Skin,
                portalAlias.BrowserType.ToString(),
                portalAlias.IsPrimary,
                UserController.Instance.GetCurrentUserInfo().UserID);

            // Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_UPDATED);

            // clear portal alias cache
            ClearCache(false);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal Dictionary<string, PortalAliasInfo> GetPortalAliasesInternal()
        {
            return CBO.GetCachedObject<Dictionary<string, PortalAliasInfo>>(
                new CacheItemArgs(
                DataCache.PortalAliasCacheKey,
                DataCache.PortalAliasCacheTimeOut,
                DataCache.PortalAliasCachePriority),
                c =>
                {
                    var dic = CBO.FillDictionary<string, PortalAliasInfo>(
                        "HTTPAlias",
                        DataProvider.Instance().GetPortalAliases());
                    return dic.Keys.ToDictionary(key => key.ToLowerInvariant(), key => dic[key]);
                },
                true);
        }

        /// <inheritdoc/>
        protected override Func<IPortalAliasController> GetFactory()
        {
            return () => new PortalAliasController();
        }

        private static void ClearCache(bool refreshServiceRoutes, int portalId = -1)
        {
            DataCache.RemoveCache(DataCache.PortalAliasCacheKey);
            CacheController.FlushPageIndexFromCache();

            if (portalId > Null.NullInteger)
            {
                DataCache.ClearTabsCache(portalId);
            }

            if (refreshServiceRoutes)
            {
                ServicesRoutingManager.ReRegisterServiceRoutesWhileSiteIsRunning();
            }
        }

        private static void LogEvent(IPortalAliasInfo portalAlias, EventLogController.EventLogType logType)
        {
            int userId = UserController.Instance.GetCurrentUserInfo().UserID;
            EventLogController.Instance.AddLog(portalAlias, PortalController.Instance.GetCurrentSettings(), userId, string.Empty, logType);
        }

        private static bool ValidateAlias(string portalAlias, bool ischild, bool isDomain)
        {
            string validChars = "abcdefghijklmnopqrstuvwxyz0123456789-/";
            if (!ischild)
            {
                validChars += ".:";
            }

            if (!isDomain)
            {
                validChars += "_";
            }

            return portalAlias.All(c => validChars.Contains(c.ToString()));
        }

        private PortalAliasInfo GetPortalAliasLookupInternal(string alias)
        {
            return this.GetPortalAliasesInternal().SingleOrDefault(pa => pa.Key == alias).Value;
        }

        private IPortalAliasInfo GetPortalAliasInternal(string httpAlias)
        {
            string strPortalAlias;

            // try the specified alias first
            IPortalAliasInfo portalAlias = this.GetPortalAliasLookupInternal(httpAlias.ToLowerInvariant());

            // domain.com and www.domain.com should be synonymous
            if (portalAlias == null)
            {
                if (httpAlias.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase))
                {
                    // try alias without the "www." prefix
                    strPortalAlias = httpAlias.Replace("www.", string.Empty);
                }
                else
                {
                    // try the alias with the "www." prefix
                    strPortalAlias = string.Concat("www.", httpAlias);
                }

                // perform the lookup
                portalAlias = this.GetPortalAliasLookupInternal(strPortalAlias.ToLowerInvariant());
            }

            // allow domain wildcards
            if (portalAlias == null)
            {
                // remove the domain prefix ( ie. anything.domain.com = domain.com )
                if (httpAlias.IndexOf(".", StringComparison.Ordinal) != -1)
                {
                    strPortalAlias = httpAlias.Substring(httpAlias.IndexOf(".", StringComparison.Ordinal) + 1);
                }
                else
                {
                    // be sure we have a clean string (without leftovers from preceding 'if' block)
                    strPortalAlias = httpAlias;
                }

                // try an explicit lookup using the wildcard entry ( ie. *.domain.com )
                portalAlias = this.GetPortalAliasLookupInternal("*." + strPortalAlias.ToLowerInvariant()) ??
                              this.GetPortalAliasLookupInternal(strPortalAlias.ToLowerInvariant());

                if (portalAlias == null)
                {
                    // try a lookup using "www." + raw domain
                    portalAlias = this.GetPortalAliasLookupInternal("www." + strPortalAlias.ToLowerInvariant());
                }
            }

            if (portalAlias == null)
            {
                // check if this is a fresh install ( no alias values in collection )
                var controller = new PortalAliasController();
                var portalAliases = controller.GetPortalAliasesInternal();
                if (portalAliases.Keys.Count == 0 || (portalAliases.Count == 1 && portalAliases.ContainsKey("_default")))
                {
                    // relate the PortalAlias to the default portal on a fresh database installation
                    DataProvider.Instance().UpdatePortalAlias(httpAlias.ToLowerInvariant().Trim('/'), UserController.Instance.GetCurrentUserInfo().UserID);
                    EventLogController.Instance.AddLog(
                        "PortalAlias",
                        httpAlias,
                        PortalController.Instance.GetCurrentSettings(),
                        UserController.Instance.GetCurrentUserInfo().UserID,
                        EventLogController.EventLogType.PORTALALIAS_UPDATED);

                    // clear the cachekey "GetPortalByAlias" otherwise portalalias "_default" stays in cache after first install
                    DataCache.RemoveCache("GetPortalByAlias");

                    // try again
                    portalAlias = this.GetPortalAliasLookupInternal(httpAlias.ToLowerInvariant());
                }
            }

            return portalAlias;
        }
    }
}
