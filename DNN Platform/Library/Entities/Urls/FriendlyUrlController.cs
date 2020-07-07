// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.ClientCapability;
    using DotNetNuke.Services.Exceptions;

    public class FriendlyUrlController
    {
        private const string DisableMobileRedirectCookieName = "disablemobileredirect"; // dnn cookies
        private const string DisableRedirectPresistCookieName = "disableredirectpresist"; // dnn cookies

        private const string DisableMobileRedirectQueryStringName = "nomo";

        // google uses the same name nomo=1 means do not redirect to mobile
        private const string MobileViewSiteCookieName = "dnn_IsMobile";
        private const string DisableMobileViewCookieName = "dnn_NoMobile";

        public static FriendlyUrlSettings GetCurrentSettings(int portalId)
        {
            return new FriendlyUrlSettings(portalId);
        }

        /*
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

        public static Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls)
        {
            return GetTabs(portalId, includeStdUrls, GetCurrentSettings(portalId));
        }

        public static Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls, FriendlyUrlSettings settings)
        {
            PortalSettings portalSettings = null;

            // 716 just ignore portal settings if we don't actually need it
            if (includeStdUrls)
            {
                portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            }

            return GetTabs(portalId, includeStdUrls, portalSettings, settings);
        }

        public static Dictionary<int, TabInfo> GetTabs(int portalId, bool includeStdUrls, PortalSettings portalSettings, FriendlyUrlSettings settings)
        {
            // 811 : friendly urls for admin/host tabs
            var tabs = new Dictionary<int, TabInfo>();
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            var hostTabs = TabController.Instance.GetTabsByPortal(-1);

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
        /// Returns a list of http alias values where that alias is associated with a tab as a custom alias.
        /// </summary>
        /// <remarks>Aliases returned are all in lower case only.</remarks>
        /// <returns></returns>
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

        public static TabInfo GetTab(int tabId, bool addStdUrls)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return GetTab(tabId, addStdUrls, portalSettings, GetCurrentSettings(portalSettings.PortalId));
        }

        public static TabInfo GetTab(int tabId, bool addStdUrls, PortalSettings portalSettings, FriendlyUrlSettings settings)
        {
            TabInfo tab = TabController.Instance.GetTab(tabId, portalSettings.PortalId, false);
            if (addStdUrls)
            {
                // Add on the standard Urls that exist for a tab, based on settings like
                // replacing spaces, diacritic characters and languages
                // BuildFriendlyUrls(tab, true, portalSettings, settings);
            }

            return tab;
        }

        public static string CleanNameForUrl(string urlName, FriendlyUrlOptions options, out bool replacedUnwantedChars)
        {
            replacedUnwantedChars = false;

            // get options
            if (options == null)
            {
                options = new FriendlyUrlOptions();
            }

            bool convertDiacritics = options.ConvertDiacriticChars;
            Regex regexMatch = options.RegexMatchRegex;
            string replaceWith = options.PunctuationReplacement;
            bool replaceDoubleChars = options.ReplaceDoubleChars;
            Dictionary<string, string> replacementChars = options.ReplaceCharWithChar;

            if (urlName == null)
            {
                urlName = string.Empty;
            }

            var result = new StringBuilder(urlName.Length);
            int i = 0;
            string normalisedUrl = urlName;
            if (convertDiacritics)
            {
                normalisedUrl = urlName.Normalize(NormalizationForm.FormD);
                if (!string.Equals(normalisedUrl, urlName, StringComparison.Ordinal))
                {
                    replacedUnwantedChars = true; // replaced an accented character
                }
            }

            int last = normalisedUrl.Length - 1;
            bool doublePeriod = false;
            foreach (char c in normalisedUrl)
            {
                // look for a double period in the name
                if (!doublePeriod && i > 0 && c == '.' && normalisedUrl[i - 1] == '.')
                {
                    doublePeriod = true;
                }

                // use string for manipulation
                string ch = c.ToString(CultureInfo.InvariantCulture);

                // do replacement in pre-defined list?
                if (replacementChars != null && replacementChars.ContainsKey(ch))
                {
                    // replace with value
                    ch = replacementChars[ch];
                    replacedUnwantedChars = true;
                }
                else if (convertDiacritics && CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    ch = string.Empty;
                    replacedUnwantedChars = true;
                }
                else
                {
                    // Check if ch is in the replace list
                    CheckCharsForReplace(options, ref ch, ref replacedUnwantedChars);

                    // not in replacement list, check if valid char
                    if (regexMatch.IsMatch(ch))
                    {
                        ch = string.Empty; // not a replacement or allowed char, so doesn't go into Url
                        replacedUnwantedChars = true;

                        // if we are here, this character isn't going into the output Url
                    }
                }

                // Check if the final ch is an illegal char
                CheckIllegalChars(options.IllegalChars, ref ch, ref replacedUnwantedChars);
                if (i == last)
                {
                    // 834 : strip off last character if it is a '.'
                    if (!(ch == "-" || ch == replaceWith || ch == "."))
                    {
                        // only append if not the same as the replacement character
                        result.Append(ch);
                    }
                    else
                    {
                        replacedUnwantedChars = true; // last char not added - effectively replaced with nothing.
                    }
                }
                else
                {
                    result.Append(ch);
                }

                i++; // increment counter
            }

            if (doublePeriod)
            {
                result = result.Replace("..", string.Empty);
            }

            // replace any duplicated replacement characters by doing replace twice
            // replaces -- with - or --- with -  //749 : ampersand not completed replaced
            if (replaceDoubleChars && !string.IsNullOrEmpty(replaceWith))
            {
                result = result.Replace(replaceWith + replaceWith, replaceWith);
                result = result.Replace(replaceWith + replaceWith, replaceWith);
            }

            return result.ToString();
        }

        /// <summary>
        /// Ensures that the path starts with the leading character.
        /// </summary>
        /// <param name="leading"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string EnsureLeadingChar(string leading, string path)
        {
            if (leading != null && path != null
                && leading.Length <= path.Length && leading != string.Empty)
            {
                string start = path.Substring(0, leading.Length);
                if (string.Compare(start, leading, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    // not leading with this
                    path = leading + path;
                }
            }

            return path;
        }

        public static string EnsureNotLeadingChar(string leading, string path)
        {
            if (leading != null && path != null
                && leading.Length <= path.Length && leading != string.Empty)
            {
                string start = path.Substring(0, leading.Length);
                if (string.Compare(start, leading, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // matches start, take leading off
                    path = path.Substring(leading.Length);
                }
            }

            return path;
        }

        // 737 : detect mobile and other types of browsers
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
                            if (viewMobileCookie == null)
                            {
                                response.Cookies.Add(new HttpCookie(MobileViewSiteCookieName, isMobile.ToString())
                                { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" });
                            }
                            else
                            {
                                viewMobileCookie.Value = isMobile.ToString();
                            }
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
            }
            while (!isUnique);

            return uniqueUrl;
        }

        internal static bool CanUseMobileDevice(HttpRequest request, HttpResponse response)
        {
            var canUseMobileDevice = true;
            int val;
            if (int.TryParse(request.QueryString[DisableMobileRedirectQueryStringName], out val))
            {
                // the nomo value is in the querystring
                if (val == 1)
                {
                    // no, can't do it
                    canUseMobileDevice = false;
                    var cookie = new HttpCookie(DisableMobileViewCookieName)
                    {
                        Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                    };
                    response.Cookies.Set(cookie);
                }
                else
                {
                    // check for disable mobile view cookie name
                    var cookie = request.Cookies[DisableMobileViewCookieName];
                    if (cookie != null)
                    {
                        // if exists, expire cookie to allow redirect
                        cookie = new HttpCookie(DisableMobileViewCookieName)
                        {
                            Expires = DateTime.Now.AddMinutes(-1),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        response.Cookies.Set(cookie);
                    }

                    // check the DotNetNuke cookies for allowed
                    if (request.Cookies[DisableMobileRedirectCookieName] != null
                        && request.Cookies[DisableRedirectPresistCookieName] != null) // check for cookie
                    {
                        // cookies exist, can't use mobile device
                        canUseMobileDevice = false;
                    }
                }
            }
            else
            {
                // look for disable mobile view cookie
                var cookie = request.Cookies[DisableMobileViewCookieName];
                if (cookie != null)
                {
                    canUseMobileDevice = false;
                }
            }

            return canUseMobileDevice;
        }

        /// <summary>
        /// Replaces the core IsAdminTab call which was decommissioned for DNN 5.0.
        /// </summary>
        /// <param name="tabPath">The path of the tab //admin//someothername.</param>
        /// <param name="settings"></param>
        /// <remarks>Duplicated in RewriteController.cs.</remarks>
        /// <returns></returns>
        internal static bool IsAdminTab(int portalId, string tabPath, FriendlyUrlSettings settings)
        {
            return RewriteController.IsAdminTab(portalId, tabPath, settings);
        }

        private static bool IsMobileClient()
        {
            return (HttpContext.Current.Request.Browser != null) && (ClientCapabilityProvider.Instance() != null) && ClientCapabilityProvider.CurrentClientCapability.IsMobile;
        }

        private static void CheckIllegalChars(string illegalChars, ref string ch, ref bool replacedUnwantedChars)
        {
            var resultingCh = new StringBuilder(ch.Length);
            foreach (char c in ch) // ch could contain several chars from the pre-defined replacement list
            {
                if (illegalChars.ToUpperInvariant().Contains(char.ToUpperInvariant(c)))
                {
                    replacedUnwantedChars = true;
                }
                else
                {
                    resultingCh.Append(c);
                }
            }

            ch = resultingCh.ToString();
        }

        private static void CheckCharsForReplace(FriendlyUrlOptions options, ref string ch, ref bool replacedUnwantedChars)
        {
            if (!options.ReplaceChars.ToUpperInvariant().Contains(ch.ToUpperInvariant()))
            {
                return;
            }

            if (ch != " ") // if not replacing spaces, which are implied
            {
                replacedUnwantedChars = true;
            }

            ch = options.PunctuationReplacement; // in list of replacment chars

            // If we still have a space ensure it's encoded
            if (ch == " ")
            {
                ch = options.SpaceEncoding;
            }
        }

        private static bool ValidateUrl(string url, int validateUrlForTabId, PortalSettings settings)
        {
            // Try and get a user by the url
            var user = UserController.GetUserByVanityUrl(settings.PortalId, url);
            bool isUnique = user == null;

            if (isUnique)
            {
                // Try and get a tab by the url
                int tabId = TabController.GetTabByTabPath(settings.PortalId, "//" + url, settings.CultureCode);
                isUnique = tabId == -1 || tabId == validateUrlForTabId;
            }

            if (isUnique) // check whether have a tab which use the url.
            {
                var friendlyUrlSettings = GetCurrentSettings(settings.PortalId);
                var tabs = TabController.Instance.GetTabsByPortal(settings.PortalId).AsList();

                // DNN-6492: if content localize enabled, only check tab names in current culture.
                if (settings.ContentLocalizationEnabled)
                {
                    tabs = tabs.Where(t => t.CultureCode == settings.CultureCode).ToList();
                }

                foreach (TabInfo tab in tabs)
                {
                    if (tab.TabID == validateUrlForTabId)
                    {
                        continue;
                    }

                    if (tab.TabUrls.Count == 0)
                    {
                        var baseUrl = Globals.AddHTTP(settings.PortalAlias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
                        var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                            tab,
                            baseUrl,
                            Globals.glbDefaultPage,
                            settings.PortalAlias.HTTPAlias,
                            false,
                            friendlyUrlSettings,
                            Guid.Empty);

                        var tabUrl = path.Replace(Globals.AddHTTP(settings.PortalAlias.HTTPAlias), string.Empty);

                        if (tabUrl.Equals("/" + url, StringComparison.InvariantCultureIgnoreCase))
                        {
                            isUnique = false;
                            break;
                        }
                    }
                    else if (tab.TabUrls.Any(u => u.Url.Equals("/" + url, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        isUnique = false;
                        break;
                    }
                }
            }

            return isUnique;
        }
    }
}
