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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.ClientCapability;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

using Assembly = System.Reflection.Assembly;

#endregion

namespace DotNetNuke.Entities.Urls
{
    public class FriendlyUrlController
    {
        #region Constants

        private const string DisableMobileRedirectCookieName = "disablemobileredirect"; //dnn cookies
        private const string DisableRedirectPresistCookieName = "disableredirectpresist"; //dnn cookies

        private const string DisableMobileRedirectQueryStringName = "nomo";
                             //google uses the same name nomo=1 means do not redirect to mobile

        private const string MobileViewSiteCookieName = "dnn_IsMobile";
        private const string DisableMobileViewCookieName = "dnn_NoMobile";

        #endregion

        #region Friendly Url Settings Control

        public static FriendlyUrlSettings GetCurrentSettings(int portalId)
        {
            return new FriendlyUrlSettings(portalId);
        }

        #endregion

        #region Friendly Url Provider methods

        /// <summary>
        /// Determines if the tab is excluded from FriendlyUrl Processing
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="settings"></param>
        /// <param name="rewriting">If true, we are checking for rewriting purposes, if false, we are checking for friendly Url Generating.</param>
        /// <returns></returns>
        private static bool IsExcludedFromFriendlyUrls(TabInfo tab, FriendlyUrlSettings settings, bool rewriting)
        {
            //note this is a duplicate of another method in RewriteController.cs
            bool exclude = false;
            string tabPath = (tab.TabPath.Replace("//", "/") + ";").ToLower();
            if (settings.UseBaseFriendlyUrls != null)
            {
                exclude = settings.UseBaseFriendlyUrls.ToLower().Contains(tabPath);
            }

            return exclude;
        }

        private static void SetExclusionProperties(TabInfo tab, FriendlyUrlSettings settings)
        {
            string tabPath = (tab.TabPath.Replace("//", "/") + ";").ToLower();
            tab.UseBaseFriendlyUrls = settings.UseBaseFriendlyUrls != null && settings.UseBaseFriendlyUrls.ToLower().Contains(tabPath);
        }
        /*
        /// <summary>
        /// Builds up a collection of the Friendly Urls for a tab
        /// </summary>
        /// <param name="tab">The TabInfoEx object</param>
        /// <param name="includeStdUrls">Whether to add in the redirects for the 'standard' DNN urls</param>
        /// <param name="portalSettings"></param>
        /// <param name="settings">The current friendly Url settings</param>
        /// <remarks>Updated  to insert where an ascii replacement or spaces-replaced replacement has been made (562)</remarks>
        private static void BuildFriendlyUrls(TabInfo tab, bool includeStdUrls, PortalSettings portalSettings, FriendlyUrlSettings settings)
        {
            
            //unfriendly Url
            if (includeStdUrls)
            {
                string stdUrl = Globals.glbDefaultPage + "?TabId=" + tab.TabID.ToString();
                string stdHttpStatus = "200";
                string httpAlias = portalSettings.PortalAlias.HTTPAlias;
                string defaultCulture = portalSettings.DefaultLanguage;
                var locales = LocaleController.Instance.GetLocales(portalSettings.PortalId);

                string baseFriendlyHttpStatus = "200";
                int seqNum = -1;
                //check for custom redirects
                //bool tabHasCustom200 = false;
                var culturesWithCustomUrls = new List<string>();
                if (tab.TabUrls.Count > 0)
                {
                    //there are custom redirects for this tab
                    //cycle through all and collect the list of cultures where a 
                    //custom redirect has been implemented
                    foreach (TabUrlInfo redirect in tab.TabUrls)
                    {
                        if (redirect.HttpStatus == "200" && !redirect.IsSystem)
                        {
                            //there is a custom redirect for this culture
                            //751 : use the default culture if the redirect doesn't have a valid culture set
                            string redirectCulture = redirect.CultureCode;
                            if (string.IsNullOrEmpty(redirectCulture))
                            {
                                redirectCulture = portalSettings.DefaultLanguage;
                            }

                            if (!culturesWithCustomUrls.Contains(redirectCulture))
                            {
                                culturesWithCustomUrls.Add(redirectCulture);
                            }
                        }
                    }
                }

                //add friendly urls first (sequence number goes in reverse)
                if (Host.Host.UseFriendlyUrls)
                {
                    //determine whether post process or use base by looking at current settings
                    SetExclusionProperties(tab, settings);

                    //friendly Urls are switched on
                    //std = default.aspx?tabId=xx 
                    //and page not excluded from redirects
                    bool onlyBaseUrls = tab.UseBaseFriendlyUrls;
                        //use base means only use Base Friendly Urls (searchFriendly)

                    //if not using base urls, and redirect all unfriendly, and not in the list of pages to not redirect
                    if (!onlyBaseUrls & settings.RedirectUnfriendly && !tab.DoNotRedirect)
                    {
                        //all base urls will be 301
                        baseFriendlyHttpStatus = "301";
                        stdHttpStatus = "301";
                        //default url 301'd if friendly Urls on and redirect unfriendly switch 'on'
                    }
                    var localeCodes = new List<string>();
                    if (!string.IsNullOrEmpty(tab.CultureCode))
                    {
                        //the tab culture is specified, so skip all locales and only process those for the locale
                        localeCodes.Add(tab.CultureCode);
                    }
                    else
                    {
                        localeCodes.AddRange(from Locale lc in locales.Values select lc.Code);
                    }
                    foreach (string cultureCode in localeCodes) //go through and generate the urls for each language
                    {
                        string langQs = "&language=" + cultureCode;
                        if (cultureCode == defaultCulture)
                        {
                            langQs = "";
                        }

                        var improvedFriendlyUrls = new Dictionary<string, string>();
                        //call friendly url provider to get current friendly url (uses all settings)
                        string baseFriendlyUrl = GetFriendlyUrl(tab, 
                                                                stdUrl + langQs, 
                                                                Globals.glbDefaultPage,
                                                                portalSettings.PortalAlias.HTTPAlias, 
                                                                settings);

                        if (onlyBaseUrls == false)
                        {
                            //get the improved friendly Url for this tab                    
                            //improved friendly Url = 'human friendly' url generated by Advanced Friendly Url Provider : note call is made to ignore custom redirects
                            //this temp switch is to clear out the useBaseFriendlyUrls setting.  The post-process setting means the generated 
                            //friendly url will be a base url, and then replaced later when the page is finished.  Because we want to pretend we're
                            //looking at the finished product, we clear out the value and restore it after the friendly url generation
                            string improvedFriendlyUrl = GetImprovedFriendlyUrl(tab, 
                                                                                stdUrl + langQs, 
                                                                                Globals.glbDefaultPage, 
                                                                                portalSettings, 
                                                                                true, 
                                                                                settings);

                            improvedFriendlyUrls.Add("hfurl:" + cultureCode, improvedFriendlyUrl);
                            //get any other values
                            bool autoAsciiConvert = false;
                            bool replaceSpacesWith = false;
                            if (settings.AutoAsciiConvert)
                            {
                                //check to see that the ascii conversion would actually produce a different result
                                string changedTabPath = ReplaceDiacritics(tab.TabPath);
                                if (changedTabPath != tab.TabPath)
                                {
                                    autoAsciiConvert = true;
                                }
                            }
                            if (settings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                            {
                                if (tab.TabName.Contains(" "))
                                {
                                    string tabPath = BuildTabPathWithReplacement(tab, " ", settings.ReplaceSpaceWith);
                                    if (tabPath != tab.TabPath)
                                    {
                                        replaceSpacesWith = true;
                                    }
                                }
                            }
                            if (autoAsciiConvert && replaceSpacesWith)
                            {
                                string replaceSpaceWith = settings.ReplaceSpaceWith;
                                settings.ReplaceSpaceWith = "None";
                                settings.AutoAsciiConvert = false;
                                //get one without auto ascii convert, and replace spaces off
                                string impUrl = GetImprovedFriendlyUrl(tab, 
                                                                        stdUrl + langQs, 
                                                                        Globals.glbDefaultPage,
                                                                        httpAlias, 
                                                                        true, 
                                                                        settings);
                                improvedFriendlyUrls.Add("aac:rsw:" + cultureCode, impUrl);
                                settings.AutoAsciiConvert = true;
                                //now get one with ascii convert on, and replace spaces still off
                                //impUrl = GetImprovedFriendlyUrl(tab, stdUrl, Globals.glbDefaultPage, httpAlias, true, settings);
                                //improvedFriendlyUrls.Add("aac", impUrl);
                                settings.ReplaceSpaceWith = replaceSpaceWith;
                            }
                            if (autoAsciiConvert && !replaceSpacesWith)
                            {
                                settings.AutoAsciiConvert = false;
                                //get one with auto ascii convert off
                                string impUrl = GetImprovedFriendlyUrl(tab, 
                                                                        stdUrl + langQs, 
                                                                        Globals.glbDefaultPage,
                                                                        httpAlias, 
                                                                        true, 
                                                                        settings);
                                improvedFriendlyUrls.Add("aac:" + cultureCode, impUrl);
                                settings.AutoAsciiConvert = true;
                            }
                            if (!autoAsciiConvert && replaceSpacesWith)
                            {
                                string replaceSpaceWith = settings.ReplaceSpaceWith;
                                settings.ReplaceSpaceWith = "None";
                                //get one with replace spaces off
                                string impUrl = GetImprovedFriendlyUrl(tab, 
                                                                        stdUrl + langQs, 
                                                                        Globals.glbDefaultPage,
                                                                        httpAlias, 
                                                                        true, 
                                                                        settings);
                                improvedFriendlyUrls.Add("rsw:" + cultureCode, impUrl);
                                settings.ReplaceSpaceWith = replaceSpaceWith;
                            }
                            bool tabHasCustom200 = culturesWithCustomUrls.Contains(cultureCode);
                            foreach (string key in improvedFriendlyUrls.Keys)
                            {
                                string friendlyUrl = improvedFriendlyUrls[key];
                                if (friendlyUrl != baseFriendlyUrl && friendlyUrl != "")
                                {
                                    //if the improved friendly Url is different to the base friendly Url,
                                    //then we will add it in as a 'fixed' url, except if the improved friendly Url
                                    //is actually a redirect in the first place
                                    bool found = false;
                                    foreach (TabUrlInfo redirect in tab.TabUrls)
                                    {
                                        //compare each redirect to the improved friendly Url
                                        //just in case it is the same
                                        if (String.Compare(redirect.Url, friendlyUrl, StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            found = true;
                                        }
                                    }
                                    if (!found)
                                    {
                                        //ok if hte improved friendly Url isn't a tab redirect record,
                                        //then add in the improved friendly Url as a 'fixed' url
                                        var predefinedRedirect = new TabUrlInfo
                                                                        {
                                                                            TabId = tab.TabID,
                                                                            CultureCode = cultureCode
                                                                        };
                                        if (key.StartsWith("hfurl") == false)
                                        {
                                            //this means it's not the actual url, it's either a spaces replace,
                                            //auto ascii or both output.  It should be a 301 unless redirectunfriendly
                                            //is off, or the page is excluded from redirects
                                            if (settings.RedirectUnfriendly && !tab.DoNotRedirect)
                                            {
                                                predefinedRedirect.HttpStatus = "301"; //redirect to custom url
                                            }
                                            else
                                            {
                                                predefinedRedirect.HttpStatus = "200"; //allow it to work
                                            }
                                        }
                                        else
                                        {
                                            //the hfurl key is the base human friendly url
                                            if (tabHasCustom200 && (settings.RedirectUnfriendly && !tab.DoNotRedirect))
                                            {
                                                predefinedRedirect.HttpStatus = "301";
                                            }
                                            else
                                            {
                                                predefinedRedirect.HttpStatus = "200";
                                                    //if no redirects, or not redirecting unfriendly, then 200 is OK
                                            }
                                        }
                                        predefinedRedirect.Url = friendlyUrl;
                                        predefinedRedirect.IsSystem = true;
                                        predefinedRedirect.SeqNum = seqNum;
                                        tab.TabUrls.Insert(0, predefinedRedirect);
                                        seqNum--;
                                    }
                                }
                                else
                                {
                                    //improved Friendly Url same as base Friendly Url, so we 200 this one, regardless of redirection settings
                                    if (tabHasCustom200 == false)
                                    {
                                        baseFriendlyHttpStatus = "200";
                                    }
                                }
                            }
                        }
                        //base friendly url
                        var baseFriendly = new TabUrlInfo
                                                {
                                                    TabId = tab.TabID,
                                                    HttpStatus = (settings.RedirectUnfriendly == false || IsExcludedFromFriendlyUrls(tab, settings, true)) 
                                                                        ? "200"
                                                                        : baseFriendlyHttpStatus,
                                                    CultureCode = cultureCode,
                                                    Url = baseFriendlyUrl,
                                                    IsSystem = true,
                                                    SeqNum = seqNum
                                                };
                        tab.TabUrls.Insert(0, baseFriendly);
                        seqNum--;
                    }
                    //standard url (/default.aspx?tabid=xx)
                    var std = new TabUrlInfo
                        {
                                            TabId = tab.TabID,
                                            HttpStatus = stdHttpStatus,
                                            CultureCode = (tab.CultureCode == "") ? defaultCulture : tab.CultureCode,
                                            Url = stdUrl,
                                            IsSystem = true,
                                            SeqNum = seqNum,
                                        };
                    tab.TabUrls.Insert(0, std);
                    seqNum--;
                }
            }
        }
        
        /// <summary>
        /// A reflection based call to the Friendly Url provider to get the 'base' (dnn standard) urls
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="path"></param>
        /// <param name="defaultPage"></param>
        /// <param name="httpAlias"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static string GetFriendlyUrl(TabInfo tab, string path, string defaultPage, string httpAlias,
                                              FriendlyUrlSettings settings)
        {
            List<string> messages;
            object result = CallFriendlyUrlProviderMethod("BaseFriendlyUrl", out messages, tab, path, defaultPage, httpAlias, settings);
            if (result == null)
            {
                return Globals.NavigateURL(tab.TabID);
            }
            return (string) result;
        }

        internal static string GetImprovedFriendlyUrl(TabInfo tab, string path, string defaultPage, string httpAlias,
                                                      bool ignoreCustomRedirects)
        {
            FriendlyUrlSettings settings = GetCurrentSettings(tab.PortalID);
            List<string> messages;
            return GetImprovedFriendlyUrl(tab, path, defaultPage, httpAlias, ignoreCustomRedirects, settings,
                                          out messages);
        }

        /// <summary>
        /// A reflection based call to the friendly URl Provider object.  Done like this to avoid a circular reference
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="path"></param>
        /// <param name="defaultPage"></param>
        /// <param name="httpAlias"></param>
        /// <param name="ignoreCustomRedirects"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static string GetImprovedFriendlyUrl(TabInfo tab, string path, string defaultPage, string httpAlias,
                                                      bool ignoreCustomRedirects, FriendlyUrlSettings settings)
        {
            List<string> messages;
            return GetImprovedFriendlyUrl(tab, path, defaultPage, httpAlias, ignoreCustomRedirects, settings,
                                          out messages);
        }

        internal static string GetImprovedFriendlyUrl(TabInfo tab, string path, string defaultPage, string httpAlias,
                                                      bool ignoreCustomRedirects, FriendlyUrlSettings settings,
                                                      out List<string> messages)
        {
            object result = CallFriendlyUrlProviderMethod("ImprovedFriendlyUrlWithMessages", out messages, tab, path,
                                                          defaultPage, httpAlias, ignoreCustomRedirects, settings);
            if (result != null)
            {
                return (string) result;
            }
            return "";
        }
        
        internal static string GetImprovedFriendlyUrl(TabInfo tab, string path, string defaultPage,
                                                      PortalSettings portalSettings, bool ignoreCustomRedirects,
                                                      FriendlyUrlSettings settings)
        {
            List<string> messages;
            object result = CallFriendlyUrlProviderMethod("ImprovedFriendlyUrlWithSettings", out messages, tab, path,
                                                          defaultPage, portalSettings, ignoreCustomRedirects, settings);
            if (result != null)
            {
                return (string) result;
            }
            return "";
        }

        internal static string GetImprovedFriendlyUrl(TabInfo tab, string path, string defaultPage,
                                                      PortalSettings portalSettings, bool ignoreCustomRedirects,
                                                      FriendlyUrlSettings settings, out List<string> messages)
        {
            object result = CallFriendlyUrlProviderMethod("ImprovedFriendlyUrlWithSettingsAndMessages", out messages,
                                                          tab, path, defaultPage, portalSettings, ignoreCustomRedirects,
                                                          settings);
            if (result != null)
            {
                return (string) result;
            }
            return "";
        }
        /*
        /// <summary>
        /// A reflection based called to the Friendly Url Provider object.  Done like this to avoid circular references
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="replaceCharacter"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        internal static string BuildTabPathWithReplacement(TabInfo tab, string replaceCharacter, string replaceWith)
        {
            object result = CallTabPathHelperMethod("BuildTabPathWithReplacement", tab, replaceCharacter, replaceWith);
            if (result != null)
            {
                return (string) result;
            }
            return "";
        }
        
        internal static string ReplaceDiacritics(string tabPath)
        {
            object result = CallTabPathHelperMethod("ReplaceDiacritics", tabPath);
            if (result != null)
            {
                return (string) result;
            }
            return "";
        }
        
        public static void RebuildCustomUrlDict(string reason, int portalId)
        {
            CallTabDictControllerMethod("InvalidateDictionary", reason, null, portalId);
        }

        //internal static void ProcessTestRequest(string httpMethod, Uri requestUri, UrlAction result, NameValueCollection queryString, FriendlyUrlSettings settings, out List<string> messages)
        //{
        //    //public void ProcessRequest(HttpContext context, HttpRequest request, HttpServerUtility Server, HttpResponse response, bool useFriendlyUrls, string requestType, Uri requestUri, UrlAction result, NameValueCollection queryStringCol, FriendlyUrlSettings settings)
        //    bool useFriendlyUrls = (DotNetNuke.Entities.Host.HostSettings.GetHostSetting("UseFriendlyUrls") == "Y");
        //    object retval = CallUrlRewriterMethod("ProcessTestRequest", out messages, useFriendlyUrls, httpMethod, requestUri, result, queryString, settings);
        //}
        /// <summary>
        /// Gets the Reflection MethodInfo object of the FriendlyUrlProvider method, 
        /// as LONG as the iFInity.UrlMaster.FriendlyUrlProvider can be found
        /// </summary>
        /// <remarks>This is a heavyweight proc, don't call too much!</remarks>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static object CallFriendlyUrlProviderMethod(string methodName, out List<string> messages,
                                                            params object[] parameters)
        {
            return CallFriendlyUrlProviderDllMethod(methodName, "iFinity.DNN.Modules.UrlMaster.DNNFriendlyUrlProvider",
                                                    out messages, parameters);
        }

        private static void CallTabDictControllerMethod(string methodName, params object[] parameters)
        {
            CallFriendlyUrlProviderDllMethod(methodName, "iFinity.DNN.Modules.UrlMaster.TabDictController",
                                             parameters);
        }

        private static object CallTabPathHelperMethod(string methodName, params object[] parameters)
        {
            return CallFriendlyUrlProviderDllMethod(methodName, "iFinity.DNN.Modules.UrlMaster.TabPathHelper",
                                                    parameters);
        }

        private static object CallFriendlyUrlProviderDllMethod(string methodName, string typeName,
                                                               params object[] parameters)
        {
            List<string> messages;
            return CallFriendlyUrlProviderDllMethod(methodName, typeName, out messages, parameters);
        }

        private static object CallFriendlyUrlProviderDllMethod(string methodName, string typeName,
                                                               out List<string> messages, params object[] parameters)
        {
            object result = null;
            messages = null;
            try
            {
                object providerObj = null;

                Assembly urlMasterProvider = Assembly.Load("iFinity.UrlMaster.FriendlyUrlProvider");
                Type[] types = urlMasterProvider.GetTypes();
                foreach (Type type in types)
                {
                    if ((type.FullName == typeName) & (type.IsClass))
                    {
                        Type providerType = type;
                        string providerTypeName = providerType.Name;
                        //570 : check to see if it is an abstract class before trying to instantiate the 
                        //calling object
                        if (!providerType.IsAbstract)
                        {
                            // get the provider objects from the stored collection if necessary
                            if (_providerObjects != null)
                            {
                                if (_providerObjects.ContainsKey(providerTypeName))
                                {
                                    providerObj = _providerObjects[providerTypeName];
                                }
                            }
                            if (providerObj == null)
                            {
                                providerObj = Activator.CreateInstance(providerType);
                            }

                            if (_providerObjects == null)
                            {
                                _providerObjects = new Dictionary<string, object> {{providerTypeName, providerObj}};
                            }
                        }

                        if (providerObj != null || providerType.IsAbstract)
                        {
                            MethodInfo method = providerType.GetMethod(methodName);
                            if (method != null)
                            {
                                //new collection 
                                int messageParmIdx = -1;
                                var parmValues = new List<object>(parameters);
                                ParameterInfo[] methodParms = method.GetParameters();
                                for (int i = 0; i <= methodParms.GetUpperBound(0); i++)
                                {
                                    if (methodParms[i].IsOut && i > parameters.GetUpperBound(0) &&
                                        methodParms[i].Name.ToLower() == "messages")
                                    {
                                        //add on another one on the end
                                        parmValues.Add(messages);
                                        messageParmIdx = i;
                                        parameters = parmValues.ToArray();
                                    }
                                    if (methodParms[i].Name.ToLower() == "parenttraceid" &&
                                        i > parameters.GetUpperBound(0))
                                    {
                                        parmValues.Add(Guid.Empty);
                                        parameters = parmValues.ToArray();
                                    }
                                }
                                result = method.Invoke(providerObj, parameters);
                                //get the out messages value
                                if (messageParmIdx > -1)
                                {
                                    messages = (List<string>) parameters[messageParmIdx];
                                }
                            }
                            break;
                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                //get the standard DNN friendly Url by loading up HttpModules
                if (messages == null)
                {
                    messages = new List<string>();
                }
                messages.Add("Error:[" + ex.Message + "]");
                if (ex.InnerException != null)
                {
                    messages.Add("Inner Error:[ " + ex.InnerException.Message + "]");
                    messages.Add("Stack Trace :[ " + ex.InnerException.StackTrace + "]");
                }
            }
            return result;
        }
        */
        #endregion
  
        #region Database methods

        public static Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls)
        {
            return GetTabs(portalId, includeStdUrls, GetCurrentSettings(portalId));
        }

        public static Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls, FriendlyUrlSettings settings)
        {
            PortalSettings portalSettings = null;
            //716 just ignore portal settings if we don't actually need it 
            if (includeStdUrls)
            {
                portalSettings = PortalController.GetCurrentPortalSettings();
            }
            return GetTabs(portalId, includeStdUrls, portalSettings, settings);
        }

        public static Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls, PortalSettings portalSettings, FriendlyUrlSettings settings)
        {
            var tc = new TabController();
            //811 : friendly urls for admin/host tabs
            var tabs = new Dictionary<int, TabInfo>();
            var portalTabs = tc.GetTabsByPortal(portalId);
            var hostTabs = tc.GetTabsByPortal(-1);

            foreach (TabInfo tab in portalTabs.Values)
            {
                tabs[tab.TabID] = tab;
            }

            if (settings.FriendlyAdminHostUrls)
            {
                foreach (TabInfo tab in hostTabs.Values)
                {
                    tabs[tab.TabID] = tab;
                }
            }
            return tabs;
        }

        /// <summary>
        /// Returns a list of http alias values where that alias is associated with a tab as a custom alias
        /// </summary>
        /// <remarks>Aliases returned are all in lower case only</remarks>
        public static List<string> GetCustomAliasesForTabs()
        {
            var aliases = new List<string>();

            IDataReader dr = DataProvider.Instance().GetCustomAliasesForTabs();
            try
            {
                while (dr.Read())
                {
                    aliases.Add(Null.SetNullString(dr["HttpAlias"]));
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
            return aliases;
        }

        /*
        internal static Dictionary<int, TabInfo> GetTabs(int portalId)
        {
            var tabs = (Dictionary<int, TabInfo>)HttpContext.Current.Session["tabs" + portalId.ToString()] ??
                       new Dictionary<int, TabInfo>();
            return tabs;
        }

        internal static void StashTabs(Dictionary<int, TabInfo> tabs, int portalId)
        {
            HttpContext.Current.Session["tabs" + portalId.ToString()] = tabs;
            var portals = (List<int>) HttpContext.Current.Session["tabportals"] ?? new List<int>();
            if (portals.Contains(portalId) == false)
            {
                portals.Add(portalId);
            }
            HttpContext.Current.Session["tabportals"] = portals;
        }

        internal static void FlushTabs()
        {
            var portals = (List<int>) HttpContext.Current.Session["tabportals"];
            if (portals != null)
            {
                foreach (int portalId in portals)
                {
                    HttpContext.Current.Session.Remove("tabs" + portalId.ToString());
                }
            }
            HttpContext.Current.Session.Remove("tabportals");
        }

        internal static void FlushTabs(int PortalId)
        {
            HttpContext.Current.Session["tabs" + PortalId.ToString()] = null;
            var portals = (List<int>) HttpContext.Current.Session["tabportals"];
            if (portals != null && portals.Contains(PortalId))
            {
                portals.Remove(PortalId);
            }
            HttpContext.Current.Session["tabportals"] = portals;
        }
        */
        public static TabInfo GetTab(int tabId, bool addStdUrls)
        {
            PortalSettings portalSettings = PortalController.GetCurrentPortalSettings();
            return GetTab(tabId, addStdUrls, portalSettings, GetCurrentSettings(portalSettings.PortalId));
        }

        public static TabInfo GetTab(int tabId, bool addStdUrls, PortalSettings portalSettings, FriendlyUrlSettings settings)
        {
            var tc = new TabController();
            TabInfo tab = tc.GetTab(tabId, portalSettings.PortalId, false);
            if (addStdUrls)
            {
                //Add on the standard Urls that exist for a tab, based on settings like
                //replacing spaces, diacritic characters and languages
                //BuildFriendlyUrls(tab, true, portalSettings, settings);
            }
            return tab;
        }

        //internal static void RebuildStandardUrls(TabInfo tab, PortalSettings portalSettings,
        //                                         FriendlyUrlSettings settings)
        //{
        //    if (tab != null)
        //    {
        //        var standardRedirects = new List<string>();
        //        foreach (TabRedirectInfo redirect in tab.TabRedirects)
        //        {
        //            if (redirect.FixedUrl)
        //            {
        //                standardRedirects.Add(redirect.Key);
        //            }
        //        }
        //        //now remove the standard redirects
        //        foreach (string key in standardRedirects)
        //        {
        //            tab.TabRedirects.Remove(key);
        //        }
        //    }
        //    BuildFriendlyUrls(tab, true, portalSettings, settings);
        //}

        //public static void RecheckRedirectsForTab(TabInfo tab, string changingKey)
        //{
        //    RecheckRedirectsForTab(tab, changingKey, GetCurrentSettings(false, true, tab.PortalID));
        //}

        //public static void RecheckRedirectsForTab(TabInfo tab, string changingKey, FriendlyUrlSettings settings)
        //{
        //    //essentially the smae logic as in BuildFriendlyUrl, but slightly different as we aren't building
        //    //up the standard urls, just processing them to make sure they obey the 301 redirect settings
        //    var cultureHasCustomRedirectWith200 = new Dictionary<string, bool>();
        //    //cycle through all of the custom redirects, looking to see for the different culture codes/custom redirects
        //    foreach (TabRedirectInfo redirect in tab.TabRedirects)
        //    {
        //        if (redirect.FixedUrl == false)
        //        {
        //            if (redirect.HttpStatus == "200")
        //            {
        //                if (!cultureHasCustomRedirectWith200.ContainsKey(redirect.CultureCode))
        //                {
        //                    cultureHasCustomRedirectWith200.Add(redirect.CultureCode, true);
        //                }
        //            }
        //        }
        //    }
        //    //all the 'fixed' urls should be 301'd
        //    TabRedirectInfo bestRedirect = null;
        //    var bestRedirects = new Dictionary<string, TabRedirectInfo>();
        //    foreach (TabRedirectInfo redirect in tab.TabRedirects)
        //    {
        //        //see if this culture has a custom redirect
        //        bool customRedirectWith200 = false;
        //        if (cultureHasCustomRedirectWith200.ContainsKey(redirect.CultureCode))
        //        {
        //            customRedirectWith200 = cultureHasCustomRedirectWith200[redirect.CultureCode];
        //        }

        //        if (customRedirectWith200 && settings.RedirectUnfriendly)
        //        {
        //            if (redirect.FixedUrl && redirect.HttpStatus == "200")
        //            {
        //                redirect.HttpStatus = "301";
        //            }
        //        }

        //        if (customRedirectWith200 == false)
        //            //no custom urls with 200 status on them (ie,on this page, no custom urls which are not redirects)
        //        {
        //            if (redirect.FixedUrl && settings.RedirectUnfriendly == false)
        //            {
        //                redirect.HttpStatus = "200"; //no redirect unfriendly, so all fixed Url's are 200 status
        //            }
        //            else if (redirect.FixedUrl && settings.RedirectUnfriendly)
        //            {
        //                bestRedirect = redirect;
        //                    //redirect unfriendly, we just want to get the last fixed url, because tihs is the 'friendliest' (per the buildFriendlyUrl procedure)
        //                SetAsBestRedirect(redirect, bestRedirects);
        //            }
        //        }
        //        else
        //        {
        //            if (!redirect.FixedUrl && changingKey != null && redirect.Key == changingKey &&
        //                redirect.HttpStatus == "200")
        //            {
        //                //not a fixed url (custom redirect), the changing key is known, and this is the item that 
        //                //the key change was for, and this is a 200, well : we have our best redriect
        //                bestRedirect = redirect;
        //                SetAsBestRedirect(redirect, bestRedirects);
        //            }
        //        }
        //    }
        //    //the best redirect is the last fixed url
        //    /*
        //    if (bestRedirect != null)
        //    {
        //        bestRedirect.HttpStatus = "200";
        //        //if the best Redirect isn't a fixed url, and the redirectUnfriendly switch is on
        //        //then set all the other redirects to 301's
        //        if (bestRedirect.FixedUrl == false && settings.RedirectUnfriendly)
        //        {
        //            foreach (TabRedirectInfo redirect in tab.TabRedirects)
        //            {
        //                //any custom redirect that isn't the chosen redirect and
        //                //is http status 200 will be changed to 301
        //                if (redirect.FixedUrl == false 
        //                && redirect.Key != bestRedirect.Key 
        //                && redirect.HttpStatus == "200" )
        //                {
        //                    //final check : this is a candidate for setting to 301 redirect, unless it's a different culture code to the bestRedirect
        //                    if (redirect.CultureCode == bestRedirect.CultureCode)
        //                        redirect.HttpStatus = "301";
        //                    //else
        //                        //don't change httpStatus for redirects in a different culture code to the changing one
        //                }
        //            }
        //        }
        //    }*/
        //    if (bestRedirects.Count > 0)
        //    {
        //        foreach (string cultureCode in bestRedirects.Keys)
        //        {
        //            //if the best redirect isn't a fixed url, and the redirectUnfriendly switch is on
        //            //every other url for this culture is a 301 redirect to the bestFriendlyUrl for this culture
        //            TabRedirectInfo bestRedirectForCulture = bestRedirects[cultureCode];
        //            foreach (TabRedirectInfo redirect in tab.TabRedirects)
        //            {
        //                if (redirect.CultureCode == cultureCode //same culture
        //                    && redirect.Key != bestRedirectForCulture.Key // not the best redirect 
        //                    && redirect.HttpStatus == "200" // not already set as redirect
        //                    && redirect.FixedUrl == false) //not a fixed url
        //                {
        //                    //change this to a 301
        //                    redirect.HttpStatus = "301";
        //                }
        //                else if (redirect.CultureCode == cultureCode //same culture
        //                         && redirect.Key == bestRedirectForCulture.Key //is the best redirect
        //                         && redirect.HttpStatus != "200") //not 200 status
        //                {
        //                    redirect.HttpStatus = "200";
        //                }
        //            }
        //        }
        //    }
        //}

        //private static void SetAsBestRedirect(TabRedirectInfo redirect,
        //                                      Dictionary<string, TabRedirectInfo> bestRedirects)
        //{
        //    string culture = redirect.CultureCode;
        //    if (bestRedirects.ContainsKey(culture))
        //    {
        //        bestRedirects[culture] = redirect;
        //    }
        //    else
        //    {
        //        bestRedirects.Add(culture, redirect);
        //    }
        //}

        ///// <summary>
        ///// Saves any changes to the Tabs objects
        ///// </summary>
        ///// <param name="tabs"></param>
        //public static void Save(Dictionary<int, TabInfo> tabs)
        //{
        //    throw new NotImplementedException("Tabs Save");
        //}

        #endregion

        private static bool IsMobileClient()
        {
            return (HttpContext.Current.Request.Browser != null) && (ClientCapabilityProvider.Instance() != null) && ClientCapabilityProvider.CurrentClientCapability.IsMobile;
        }

        #region Internal Methods

        internal static bool CanUseMobileDevice(HttpRequest request, HttpResponse response)
        {
            bool canUseMobileDevice = true;
            //if (int.TryParse(app.Request.QueryString[DisableMobileRedirectQueryStringName], out val))
            //{
            //    if (val == 0) //forced enable. clear any cookie previously set
            //    {
            //        if (app.Response.Cookies[DisableMobileRedirectCookieName] != null)
            //        {
            //            HttpCookie cookie = new HttpCookie(DisableMobileRedirectCookieName);
            //            cookie.Expires = DateTime.Now.AddMinutes(-1);
            //            app.Response.Cookies.Add(cookie);
            //        }

            //        if (app.Response.Cookies[DisableRedirectPresistCookieName] != null)
            //        {
            //            HttpCookie cookie = new HttpCookie(DisableRedirectPresistCookieName);
            //            cookie.Expires = DateTime.Now.AddMinutes(-1);
            //            app.Response.Cookies.Add(cookie);
            //        }
            //    }
            //    else if (val == 1) //forced disable. need to setup cookie
            //    {
            //        allowed = false;
            //    }
            //}                                  
            int val;
            if (int.TryParse(request.QueryString[DisableMobileRedirectQueryStringName], out val))
            {
                //the nomo value is in the querystring
                if (val == 1)
                {
                    //no, can't do it
                    canUseMobileDevice = false;
                    var cookie = new HttpCookie(DisableMobileViewCookieName);
                    response.Cookies.Set(cookie);
                }
                else
                {
                    //check for disable mobile view cookie name
                    HttpCookie cookie = request.Cookies[DisableMobileViewCookieName];
                    if (cookie != null)
                    {
                        //if exists, expire cookie to allow redirect
                        cookie = new HttpCookie(DisableMobileViewCookieName) { Expires = DateTime.Now.AddMinutes(-1) };
                        response.Cookies.Set(cookie);
                    }
                    //check the DotNetNuke cookies for allowed
                    if (request.Cookies[DisableMobileRedirectCookieName] != null
                        && request.Cookies[DisableRedirectPresistCookieName] != null) //check for cookie
                    {
                        //cookies exist, can't use mobile device
                        canUseMobileDevice = false;
                    }
                }
            }
            else
            {
                //look for disable mobile view cookie
                HttpCookie cookie = request.Cookies[DisableMobileViewCookieName];
                if (cookie != null)
                {
                    canUseMobileDevice = false;
                }
            }

            return canUseMobileDevice;
        }

        /// <summary>
        /// Replaces the core IsAdminTab call which was decommissioned for DNN 5.0
        /// </summary>
        /// <param name="tabPath">The path of the tab //admin//someothername</param>
        /// <param name="settings"></param>
        /// <remarks>Duplicated in RewriteController.cs</remarks>
        /// <returns></returns>
        internal static bool IsAdminTab(int portalId, string tabPath, FriendlyUrlSettings settings)
        {
            //fallback position - all portals match 'Admin'
            const string adminPageName = "Admin";
            //we should be checking that the tab path matches //Admin//pagename or //admin
            //in this way we should avoid partial matches (ie //Administrators
            if (tabPath.StartsWith("//" + adminPageName + "//", StringComparison.CurrentCultureIgnoreCase)
                || String.Compare(tabPath, "//" + adminPageName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Public Methods

        public static string CleanNameForUrl(string urlName, FriendlyUrlOptions options, out bool replacedUnwantedChars)
        {
            replacedUnwantedChars = false;
            //get options
            if (options == null)
            {
                options = new FriendlyUrlOptions();
            }
            bool convertDiacritics = options.ConvertDiacriticChars;
            string regexMatch = options.RegexMatch;
            string illegalChars = options.IllegalChars;
            string replaceWith = options.PunctuationReplacement;
            string replaceChars = options.ReplaceChars;
            bool replaceDoubleChars = options.ReplaceDoubleChars;
            Dictionary<string, string> replacementChars = options.ReplaceCharWithChar;

            if (urlName == null)
            {
                urlName = "";
            }
            var result = new StringBuilder(urlName.Length);
            int i = 0;
            int last = urlName.ToCharArray().GetUpperBound(0);
            string normalisedUrl = urlName;
            if (convertDiacritics)
            {
                normalisedUrl = urlName.Normalize(NormalizationForm.FormD);
                if (string.CompareOrdinal(normalisedUrl, urlName) != 0)
                {
                    replacedUnwantedChars = true; //replaced an accented character
                }
            }
            bool doublePeriod = false;
            foreach (char c in normalisedUrl)
            {
                //look for a double period in the name
                if (!doublePeriod && c == '.' && i > 0 && urlName[i - 1] == '.')
                {
                    doublePeriod = true;
                }
                //use string for manipulation
                string ch = c.ToString();
                //do replacement in pre-defined list?
                if (replacementChars != null && replacementChars.ContainsKey(c.ToString()))
                {
                    //replace with value
                    ch = replacementChars[c.ToString()];
                    replacedUnwantedChars = true;
                }
                else
                {
                    //not in replacement list, check if valid char
                    if (Regex.IsMatch(ch, regexMatch, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        //check replace character with the punctuation replacmenet
                        if (replaceChars.ToLower().Contains(ch.ToLower()))
                        {
                            ch = replaceWith; //in list of replacment chars
                            if (ch != " ") // if not replacing spaces, which are implied
                            {
                                replacedUnwantedChars = true;
                            }
                        }
                        else
                        {
                            ch = ""; //not a replacement or allowed char, so doesn't go into Url
                            replacedUnwantedChars = true;
                            //if we are here, this character isn't going into the output Url
                        }
                    }
                    else
                    {
                        //this char is allowed because it didn't match the regexMatch pattern
                        //however we may still want to replace it in Urls
                        if (illegalChars.ToLower().Contains(ch.ToLower()))
                        {
                            ch = ""; //illegal character, removed from list
                            replacedUnwantedChars = true;
                        }
                        else
                        {
                            //but we also want to check the list of illegal chars - these must never be allowed,
                            //even if the settings inadvertently let them through.  This is a double check
                            //to prevent accidental modification to regex taking down a site
                            if (replaceChars.ToLower().Contains(ch.ToLower()))
                            {
                                if (ch != " ") // if not replacing spaces, which are implied
                                {
                                    replacedUnwantedChars = true;
                                }
                                ch = replaceWith; //in list of replacment chars
                                
                                //If we still have a space ensure its encoded
                                if (ch == " ")
                                {
                                    ch = options.SpaceEncoding;
                                }
                            }
                        }
                    }
                }

                if (i == last)
                {
                    //834 : strip off last character if it is a '.'
                    if (!(ch == "-" || ch == replaceWith || ch == "."))
                    {
                        //only append if not the same as the replacement character
                        result.Append(ch);
                    }
                    else
                    {
                        replacedUnwantedChars = true; //last char not added - effectively replaced with nothing.
                    }
                }
                else
                {
                    result.Append(ch);
                }
                i++; //increment counter
            }

            if (doublePeriod)
            {
                result = result.Replace("..", "");
            }
            //replace any duplicated replacement characters by doing replace twice
            //replaces -- with - or --- with -  //749 : ampersand not completed replaced
            if (replaceDoubleChars && !string.IsNullOrEmpty(replaceWith))
            {
                result = result.Replace(replaceWith + replaceWith, replaceWith);
                result = result.Replace(replaceWith + replaceWith, replaceWith);
            }

            return result.ToString();
        }

        /// <summary>
        /// Ensures that the path starts with the leading character
        /// </summary>
        /// <param name="leading"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string EnsureLeadingChar(string leading, string path)
        {
            if (leading != null && path != null
                && leading.Length <= path.Length && leading != "")
            {
                string start = path.Substring(0, leading.Length);
                if (String.Compare(start, leading, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    //not leading with this 
                    path = leading + path;
                }
            }
            return path;
        }

        public static string EnsureNotLeadingChar(string leading, string path)
        {
            if (leading != null && path != null
                && leading.Length <= path.Length && leading != "")
            {
                string start = path.Substring(0, leading.Length);
                if (String.Compare(start, leading, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    //matches start, take leading off
                    path = path.Substring(leading.Length);
                }
            }
            return path;
        }

        //737 : detect mobile and other types of browsers
        public static BrowserTypes GetBrowserType(HttpRequest request, HttpResponse response, FriendlyUrlSettings settings)
        {
            var browserType = BrowserTypes.Normal;
            if (request != null && settings != null)
            {
                bool isCookieSet = false;
                bool isMobile = false;
                if (CanUseMobileDevice(request, response))
                {
                    HttpCookie viewMobileCookie = response.Cookies[MobileViewSiteCookieName];
                    if (viewMobileCookie != null && bool.TryParse(viewMobileCookie.Value, out isMobile))
                    {
                        isCookieSet = true;
                    }
                    if (isMobile == false)
                    {
                        if (!isCookieSet)
                        {
                            isMobile = IsMobileClient();
                            if (isMobile)
                            {
                                browserType = BrowserTypes.Mobile;
                            }

                            // Store the result as a cookie.
                            response.Cookies.Set(new HttpCookie(MobileViewSiteCookieName, isMobile.ToString()));
                        }
                    }
                    else
                    {
                        browserType = BrowserTypes.Mobile;
                    }
                }
            }
            return browserType;
        }

        public static string ValidateUrl(string cleanUrl, int validateUrlForTabId, PortalSettings settings, out bool modified)
        {
            modified = false;
            bool isUnique;
            var uniqueUrl = cleanUrl;
            int counter = 0;
            do
            {
                if (counter > 0)
                {
                    uniqueUrl = uniqueUrl + counter.ToString(CultureInfo.InvariantCulture);
                    modified = true;
                }
                isUnique = ValidateUrl(uniqueUrl, validateUrlForTabId, settings);
                counter++;
            } while (!isUnique);

            return uniqueUrl;
        }

        private static bool ValidateUrl(string url, int validateUrlForTabId, PortalSettings settings)
        {
            bool isUnique = true;

            //Try and get a user by the url
            var user = UserController.GetUserByVanityUrl(settings.PortalId, url);
            isUnique = (user == null);

            if (isUnique)
            {
                //Try and get a tab by the url
                int tabId = TabController.GetTabByTabPath(settings.PortalId, "//" + url, settings.CultureCode);
                isUnique = (tabId == -1 || tabId == validateUrlForTabId);
            }
            return isUnique;
        }


        #endregion
    }
}