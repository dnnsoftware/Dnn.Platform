#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Data;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Cache;
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
    public class PortalAliasController
    {
        #region Private Methods

        private static object GetPortalAliasLookupCallBack(CacheItemArgs cacheItemArgs)
        {
	        return TestablePortalAliasController.Instance.GetPortalAliases();
        }

        private static bool ValidateAlias(string portalAlias, bool ischild, bool isDomain)
        {
            bool isValid = true;

            string validChars = "abcdefghijklmnopqrstuvwxyz0123456789-/";
            if (!ischild)
            {
                validChars += ".:";
            }

            if (!isDomain)
            {
                validChars += "_";
            }

            foreach (char c in portalAlias)
            {
                if (!validChars.Contains(c.ToString()))
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the portal alias.
        /// </summary>
        /// <param name="PortalAlias">The portal alias.</param>
        /// <param name="PortalID">The portal ID.</param>
        /// <returns>Portal Alias Info.</returns>
        public PortalAliasInfo GetPortalAlias(string PortalAlias, int PortalID)
        {
            return (PortalAliasInfo)CBO.FillObject(DataProvider.Instance().GetPortalAlias(PortalAlias, PortalID), typeof(PortalAliasInfo));
        }

        /// <summary>
        /// Gets the portal alias by portal alias ID.
        /// </summary>
        /// <param name="PortalAliasID">The portal alias ID.</param>
        /// <returns>Portal alias info.</returns>
        public PortalAliasInfo GetPortalAliasByPortalAliasID(int PortalAliasID)
        {
            return (PortalAliasInfo)CBO.FillObject((DataProvider.Instance().GetPortalAliasByPortalAliasID(PortalAliasID)), typeof(PortalAliasInfo));
        }

        /// <summary>
        /// Gets the portal by portal alias ID.
        /// </summary>
        /// <param name="PortalAliasId">The portal alias id.</param>
        /// <returns>Portal info.</returns>
        public PortalInfo GetPortalByPortalAliasID(int PortalAliasId)
        {
            return (PortalInfo)CBO.FillObject(DataProvider.Instance().GetPortalByPortalAliasID(PortalAliasId), typeof(PortalInfo));
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the portal alias by portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        public static string GetPortalAliasByPortal(int portalId, string portalAlias)
        {
            string retValue = "";

            //get the portal alias collection from the cache
            var portalAliasCollection = TestablePortalAliasController.Instance.GetPortalAliases();
            string httpAlias;
            bool foundAlias = false;

            //Do a specified PortalAlias check first
            PortalAliasInfo portalAliasInfo;
            if (portalAliasCollection.ContainsKey(portalAlias))
            {
                portalAliasInfo = portalAliasCollection[portalAlias.ToLower()];
                if (portalAliasInfo != null)
                {
                    if (portalAliasInfo.PortalID == portalId)
                    {
                        //set the alias
                        retValue = portalAliasInfo.HTTPAlias;
                        foundAlias = true;
                    }
                }
            }


            if (!foundAlias)
            {
                //searching from longest to shortest alias ensures that the most specific portal is matched first
                //In some cases this method has been called with "portalaliases" that were not exactly the real portal alias
                //the startswith behaviour is preserved here to support those non-specific uses
                IEnumerable<String> aliases = portalAliasCollection.Keys.Cast<String>().OrderByDescending(k => k.Length);
                foreach (var currentAlias in aliases)
                {
                    // check if the alias key starts with the portal alias value passed in - we use
                    // StartsWith because child portals are redirected to the parent portal domain name
                    // eg. child = 'www.domain.com/child' and parent is 'www.domain.com'
                    // this allows the parent domain name to resolve to the child alias ( the tabid still identifies the child portalid )
                    if (portalAliasCollection.ContainsKey(currentAlias))
                    {
                        portalAliasInfo = portalAliasCollection[currentAlias];
                        httpAlias = portalAliasInfo.HTTPAlias.ToLower();
                        if (httpAlias.StartsWith(portalAlias.ToLower()) && portalAliasInfo.PortalID == portalId)
                        {
                            retValue = portalAliasInfo.HTTPAlias;
                            break;
                        }
                        httpAlias = httpAlias.StartsWith("www.") ? httpAlias.Replace("www.", "") : string.Concat("www.", httpAlias);
                        if (httpAlias.StartsWith(portalAlias.ToLower()) && portalAliasInfo.PortalID == portalId)
                        {
                            retValue = portalAliasInfo.HTTPAlias;
                            break;
                        }
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// Gets the portal alias by tab.
        /// </summary>
        /// <param name="TabID">The tab ID.</param>
        /// <param name="PortalAlias">The portal alias.</param>
        /// <returns>Portal alias.</returns>
        public static string GetPortalAliasByTab(int TabID, string PortalAlias)
        {
            string retValue = Null.NullString;
            int intPortalId = -2;

            //get the tab
            var objTabs = new TabController();
            TabInfo objTab = objTabs.GetTab(TabID, Null.NullInteger, false);
            if (objTab != null)
            {
                //ignore deleted tabs
                if (!objTab.IsDeleted)
                {
                    intPortalId = objTab.PortalID;
                }
            }
            switch (intPortalId)
            {
                case -2: //tab does not exist
                    break;
                case -1: //host tab
                    //host tabs are not verified to determine if they belong to the portal alias
                    retValue = PortalAlias;
                    break;
                default: //portal tab
                    retValue = GetPortalAliasByPortal(intPortalId, PortalAlias);
                    break;
            }
            return retValue;
        }

        /// <summary>
        /// Gets the portal alias info.
        /// </summary>
        /// <param name="httpAlias">The portal alias.</param>
        /// <returns>Portal alias info</returns>
        public static PortalAliasInfo GetPortalAliasInfo(string httpAlias)
        {
            string strPortalAlias;

            //try the specified alias first
            PortalAliasInfo portalAlias = GetPortalAliasLookup(httpAlias.ToLower());

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
                portalAlias = GetPortalAliasLookup(strPortalAlias.ToLower());
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
                portalAlias = GetPortalAliasLookup("*." + strPortalAlias.ToLower()) ??
                              GetPortalAliasLookup(strPortalAlias.ToLower());

                if (portalAlias == null)
                {
                    //try a lookup using "www." + raw domain
                    portalAlias = GetPortalAliasLookup("www." + strPortalAlias.ToLower());
                }
            }
            if (portalAlias == null)
            {
                //check if this is a fresh install ( no alias values in collection )
                var portalAliases = TestablePortalAliasController.Instance.GetPortalAliases();
                if (portalAliases.Keys.Count == 0 || (portalAliases.Count == 1 && portalAliases.ContainsKey("_default")))
                {
                    //relate the PortalAlias to the default portal on a fresh database installation
                    DataProvider.Instance().UpdatePortalAlias(httpAlias.ToLower().Trim('/'), UserController.GetCurrentUserInfo().UserID);
                    var objEventLog = new EventLogController();
                    objEventLog.AddLog("PortalAlias",
                                       httpAlias,
                                       PortalController.GetCurrentPortalSettings(),
                                       UserController.GetCurrentUserInfo().UserID,
                                       EventLogController.EventLogType.PORTALALIAS_UPDATED);

                    //clear the cachekey "GetPortalByAlias" otherwise portalalias "_default" stays in cache after first install
                    DataCache.RemoveCache("GetPortalByAlias");
                    //try again
                    portalAlias = GetPortalAliasLookup(httpAlias.ToLower());
                }
            }
            return portalAlias;
        }

        /// <summary>
        /// Gets the portal alias lookup.
        /// </summary>
        /// <param name="httpAlias">The alias.</param>
        /// <returns>Porta lAlias Info</returns>
        public static PortalAliasInfo GetPortalAliasLookup(string httpAlias)
        {
            PortalAliasInfo alias = null;
            var aliases = TestablePortalAliasController.Instance.GetPortalAliases();
            if (aliases.ContainsKey(httpAlias))
            {
                alias = TestablePortalAliasController.Instance.GetPortalAliases()[httpAlias];
            }
            return alias;
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

        #region Obsolete Methods

        [Obsolete("Deprecated in version 7.1.  Replaced by TestablePortalAliasController.Instance.AddPortalAlias")]
        public int AddPortalAlias(PortalAliasInfo portalAlias)
        {
            return TestablePortalAliasController.Instance.AddPortalAlias(portalAlias);
        }

        [Obsolete("Deprecated in version 7.1.  Replaced by TestablePortalAliasController.Instance.DeletePortalAlias")]
        public void DeletePortalAlias(int portalAliasId)
        {

            DataProvider.Instance().DeletePortalAlias(portalAliasId);

            var eventLogController = new EventLogController();
            eventLogController.AddLog("PortalAliasID",
                               portalAliasId.ToString(),
                               PortalController.GetCurrentPortalSettings(),
                               UserController.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.PORTALALIAS_DELETED);

            DataCache.RemoveCache(DataCache.PortalAliasCacheKey);
        }

        [Obsolete("Deprecated in version 7.1.  Replaced by TestablePortalAliasController.Instance.GetPortalAliasesByPortalId")]
        public ArrayList GetPortalAliasArrayByPortalID(int PortalID)
        {
            return new ArrayList(TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(PortalID).ToArray());
        }

        [Obsolete("Deprecated in version 7.1.  Replaced by TestablePortalAliasController.Instance.GetPortalAliasesByPortalId")]
        public PortalAliasCollection GetPortalAliasByPortalID(int PortalID)
        {
            var portalAliasCollection = new PortalAliasCollection();

            foreach (PortalAliasInfo alias in GetPortalAliasLookup().Values.Cast<PortalAliasInfo>().Where(alias => alias.PortalID == PortalID))
            {
                portalAliasCollection.Add(alias.HTTPAlias, alias);
            }

            return portalAliasCollection;
        }

        [Obsolete("Deprecated in version 7.1.  Replaced by TestablePortalAliasController.Instance.GetPortalAliases")]
        public PortalAliasCollection GetPortalAliases()
        {
            var portalAliasCollection = new PortalAliasCollection();
            foreach (var kvp in TestablePortalAliasController.Instance.GetPortalAliases())
            {
                portalAliasCollection.Add(kvp.Key, kvp.Value);
            }

            return portalAliasCollection;
        }

        [Obsolete("Deprecated in version 7.1.  Replaced by TestablePortalAliasController.Instance.GetPortalAliases")]
        public static PortalAliasCollection GetPortalAliasLookup()
        {
            var portalAliasCollection = new PortalAliasCollection();
            foreach (var kvp in TestablePortalAliasController.Instance.GetPortalAliases())
            {
                portalAliasCollection.Add(kvp.Key, kvp.Value);
            }

            return portalAliasCollection;
        }

        [Obsolete("Deprecated in version 7.1.  Replaced by TestablePortalAliasController.Instance.UpdatePortalAlias")]
        public void UpdatePortalAliasInfo(PortalAliasInfo portalAlias)
        {
            TestablePortalAliasController.Instance.UpdatePortalAlias(portalAlias);
        }

        #endregion
    }
}
