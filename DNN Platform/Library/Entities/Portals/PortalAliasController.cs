#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Portals
{
	/// <summary>
	/// PortalAliasController provides method to manage portal alias.
	/// </summary>
	/// <remarks>
	/// For DotNetNuke to know what site a request should load, it uses a system of portal aliases. 
	/// When a request is recieved by DotNetNuke from IIS, it extracts the domain name portion and does a comparison against 
	/// the list of portal aliases and then redirects to the relevant portal to load the approriate page. 
	/// </remarks>
    public partial class PortalAliasController : ServiceLocator<IPortalAliasController, PortalAliasController>, IPortalAliasController
    {
        protected override Func<IPortalAliasController> GetFactory()
        {
            return () => new PortalAliasController();
        }

        #region Private Methods

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

	    private PortalAliasInfo GetPortalAliasLookupInternal(string alias)
	    {
            return GetPortalAliasesInternal().SingleOrDefault(pa => pa.Key == alias).Value;
        }

        private PortalAliasInfo GetPortalAliasInternal(string httpAlias)
	    {
            string strPortalAlias;

            //try the specified alias first
            PortalAliasInfo portalAlias = GetPortalAliasLookupInternal(httpAlias.ToLower());

            //domain.com and www.domain.com should be synonymous
            if (portalAlias == null)
            {
                if (httpAlias.ToLower().StartsWith("www."))
                {
                    //try alias without the "www." prefix
                    strPortalAlias = httpAlias.Replace("www.", "");
                }
                else //try the alias with the "www." prefix
                {
                    strPortalAlias = string.Concat("www.", httpAlias);
                }
                //perform the lookup
                portalAlias = GetPortalAliasLookupInternal(strPortalAlias.ToLower());
            }
            //allow domain wildcards 
            if (portalAlias == null)
            {
                //remove the domain prefix ( ie. anything.domain.com = domain.com )
                if (httpAlias.IndexOf(".", StringComparison.Ordinal) != -1)
                {
                    strPortalAlias = httpAlias.Substring(httpAlias.IndexOf(".", StringComparison.Ordinal) + 1);
                }
                else //be sure we have a clean string (without leftovers from preceding 'if' block)
                {
                    strPortalAlias = httpAlias;
                }
                //try an explicit lookup using the wildcard entry ( ie. *.domain.com )
                portalAlias = GetPortalAliasLookupInternal("*." + strPortalAlias.ToLower()) ??
                              GetPortalAliasLookupInternal(strPortalAlias.ToLower());

                if (portalAlias == null)
                {
                    //try a lookup using "www." + raw domain
                    portalAlias = GetPortalAliasLookupInternal("www." + strPortalAlias.ToLower());
                }
            }
            if (portalAlias == null)
            {
                //check if this is a fresh install ( no alias values in collection )
                var controller = new PortalAliasController();
                var portalAliases = controller.GetPortalAliasesInternal();
                if (portalAliases.Keys.Count == 0 || (portalAliases.Count == 1 && portalAliases.ContainsKey("_default")))
                {
                    //relate the PortalAlias to the default portal on a fresh database installation
                    DataProvider.Instance().UpdatePortalAlias(httpAlias.ToLower().Trim('/'), UserController.Instance.GetCurrentUserInfo().UserID);
                    EventLogController.Instance.AddLog("PortalAlias",
                                       httpAlias,
                                       PortalController.Instance.GetCurrentPortalSettings(),
                                       UserController.Instance.GetCurrentUserInfo().UserID,
                                       EventLogController.EventLogType.PORTALALIAS_UPDATED);

                    //clear the cachekey "GetPortalByAlias" otherwise portalalias "_default" stays in cache after first install
                    DataCache.RemoveCache("GetPortalByAlias");
                    //try again
                    portalAlias = GetPortalAliasLookupInternal(httpAlias.ToLower());
                }
            }
            return portalAlias;
	        
	    }

        private static void LogEvent(PortalAliasInfo portalAlias, EventLogController.EventLogType logType)
        {
            int userId = UserController.Instance.GetCurrentUserInfo().UserID;
            EventLogController.Instance.AddLog(portalAlias, PortalController.Instance.GetCurrentPortalSettings(), userId, "", logType);
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

        #endregion

        #region Public Methods

        public int AddPortalAlias(PortalAliasInfo portalAlias)
        {
            //Add Alias
            var dataProvider = DataProvider.Instance();
            int Id = dataProvider.AddPortalAlias(portalAlias.PortalID,
                                                 portalAlias.HTTPAlias.ToLower().Trim('/'),
                                                 portalAlias.CultureCode,
                                                 portalAlias.Skin,
                                                 portalAlias.BrowserType.ToString(),
                                                 portalAlias.IsPrimary,
                                                 UserController.Instance.GetCurrentUserInfo().UserID);

            //Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_CREATED);

            //clear portal alias cache
            ClearCache(true);

            return Id;
        }

        public void DeletePortalAlias(PortalAliasInfo portalAlias)
        {
            //Delete Alias
            DataProvider.Instance().DeletePortalAlias(portalAlias.PortalAliasID);

            //Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_DELETED);

            //clear portal alias cache
            ClearCache(false, portalAlias.PortalID);
        }

        public PortalAliasInfo GetPortalAlias(string alias)
        {
            return GetPortalAliasInternal(alias);
        }

        /// <summary>
        /// Gets the portal alias.
        /// </summary>
        /// <param name="alias">The portal alias.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>Portal Alias Info.</returns>
        public PortalAliasInfo GetPortalAlias(string alias, int portalId)
        {
            return GetPortalAliasesInternal().SingleOrDefault(pa => pa.Key.Equals(alias, StringComparison.InvariantCultureIgnoreCase) && pa.Value.PortalID == portalId).Value;
        }

        /// <summary>
        /// Gets the portal alias by portal alias ID.
        /// </summary>
        /// <param name="portalAliasId">The portal alias ID.</param>
        /// <returns>Portal alias info.</returns>
        public PortalAliasInfo GetPortalAliasByPortalAliasID(int portalAliasId)
        {
            return GetPortalAliasesInternal().SingleOrDefault(pa => pa.Value.PortalAliasID == portalAliasId).Value;
        }

	    internal Dictionary<string, PortalAliasInfo> GetPortalAliasesInternal()
        {
            return CBO.GetCachedObject<Dictionary<string, PortalAliasInfo>>(new CacheItemArgs(DataCache.PortalAliasCacheKey,
                                                                                        DataCache.PortalAliasCacheTimeOut,
                                                                                        DataCache.PortalAliasCachePriority),
                                                                c =>
                                                                {
                                                                    var dic = CBO.FillDictionary<string, PortalAliasInfo>("HTTPAlias",
                                                                        DataProvider.Instance().GetPortalAliases());
                                                                    return dic.Keys.ToDictionary(key => key.ToLowerInvariant(), key => dic[key]);
                                                                },
                                                                true);
        }

	    public PortalAliasCollection GetPortalAliases()
	    {
	        var aliasCollection = new PortalAliasCollection();
	        foreach (var alias in GetPortalAliasesInternal().Values)
	        {
	            aliasCollection.Add(alias.HTTPAlias, alias);
	        }

            return aliasCollection;
        }

        public IEnumerable<PortalAliasInfo> GetPortalAliasesByPortalId(int portalId)
        {
            return GetPortalAliasesInternal().Values.Where(alias => alias.PortalID == portalId).ToList();
        }

        /// <summary>
        /// Gets the portal by portal alias ID.
        /// </summary>
        /// <param name="PortalAliasId">The portal alias id.</param>
        /// <returns>Portal info.</returns>
        public PortalInfo GetPortalByPortalAliasID(int PortalAliasId)
        {
            return CBO.FillObject<PortalInfo>(DataProvider.Instance().GetPortalByPortalAliasID(PortalAliasId));
        }

        public void UpdatePortalAlias(PortalAliasInfo portalAlias)
        {
            //Update Alias
            DataProvider.Instance().UpdatePortalAliasInfo(portalAlias.PortalAliasID,
                                                            portalAlias.PortalID,
                                                            portalAlias.HTTPAlias.ToLower().Trim('/'),
                                                            portalAlias.CultureCode,
                                                            portalAlias.Skin,
                                                            portalAlias.BrowserType.ToString(),
                                                            portalAlias.IsPrimary,
                                                            UserController.Instance.GetCurrentUserInfo().UserID);
            //Log Event
            LogEvent(portalAlias, EventLogController.EventLogType.PORTALALIAS_UPDATED);

            //clear portal alias cache
            ClearCache(false);
        }

        #endregion

        #region Public Static Helper Methods

        /// <summary>
        /// Gets the portal alias by portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        public static string GetPortalAliasByPortal(int portalId, string portalAlias)
        {
            string retValue = "";
            bool foundAlias = false;
            PortalAliasInfo portalAliasInfo = Instance.GetPortalAlias(portalAlias, portalId);
            if (portalAliasInfo != null)
            {
                retValue = portalAliasInfo.HTTPAlias;
                foundAlias = true;
            }

            if (!foundAlias)
            {
                //searching from longest to shortest alias ensures that the most specific portal is matched first
                //In some cases this method has been called with "portalaliases" that were not exactly the real portal alias
                //the startswith behaviour is preserved here to support those non-specific uses
                var controller = new PortalAliasController();
                var portalAliases = controller.GetPortalAliasesInternal();
                var portalAliasCollection = portalAliases.OrderByDescending(k => k.Key.Length);

                foreach (var currentAlias in portalAliasCollection)
                {
                    // check if the alias key starts with the portal alias value passed in - we use
                    // StartsWith because child portals are redirected to the parent portal domain name
                    // eg. child = 'www.domain.com/child' and parent is 'www.domain.com'
                    // this allows the parent domain name to resolve to the child alias ( the tabid still identifies the child portalid )
                    string httpAlias = currentAlias.Value.HTTPAlias.ToLower();
                    if (httpAlias.StartsWith(portalAlias.ToLower()) && currentAlias.Value.PortalID == portalId)
                    {
                        retValue = currentAlias.Value.HTTPAlias;
                        break;
                    }
                    httpAlias = httpAlias.StartsWith("www.") ? httpAlias.Replace("www.", "") : string.Concat("www.", httpAlias);
                    if (httpAlias.StartsWith(portalAlias.ToLower()) && currentAlias.Value.PortalID == portalId)
                    {
                        retValue = currentAlias.Value.HTTPAlias;
                        break;
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// Gets the portal alias by tab.
        /// </summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        public static string GetPortalAliasByTab(int tabId, string portalAlias)
        {
            string retValue = Null.NullString;
            int intPortalId = -2;

            //get the tab
            TabInfo tab = TabController.Instance.GetTab(tabId, Null.NullInteger);
            if (tab != null)
            {
                //ignore deleted tabs
                if (!tab.IsDeleted)
                {
                    intPortalId = tab.PortalID;
                }
            }
            switch (intPortalId)
            {
                case -2: //tab does not exist
                    break;
                case -1: //host tab
                    //host tabs are not verified to determine if they belong to the portal alias
                    retValue = portalAlias;
                    break;
                default: //portal tab
                    retValue = GetPortalAliasByPortal(intPortalId, portalAlias);
                    break;
            }
            return retValue;
        }

        /// <summary>
        /// Validates the alias.
        /// </summary>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="ischild">if set to <c>true</c> [ischild].</param>
        /// <returns><c>true</c> if the alias is a valid url format; otherwise return <c>false</c>.</returns>
        public static bool ValidateAlias(string portalAlias, bool ischild)
        {
            if (ischild)
            {
                return ValidateAlias(portalAlias, true, false);
            }
            //validate the domain
            Uri result;
            if (Uri.TryCreate(Globals.AddHTTP(portalAlias), UriKind.Absolute, out result))
            {
                return ValidateAlias(result.Host, false, true) && ValidateAlias(portalAlias, false, false);
            }
            return false;
        }

        #endregion
    }
}
