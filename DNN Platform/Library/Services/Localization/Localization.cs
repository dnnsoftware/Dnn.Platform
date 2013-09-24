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
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;

using DotNetNuke.Collections.Internal;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization.Internal;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;
using System.Text.RegularExpressions;

#endregion

namespace DotNetNuke.Services.Localization
{
    /// <summary>
    /// <para>CultureDropDownTypes allows the user to specify which culture name is displayed in the drop down list that is filled 
    /// by using one of the helper methods.</para>
    /// </summary>
    [Serializable]
    public enum CultureDropDownTypes
    {
        /// <summary>
        /// Displays the culture name in the format "&lt;languagefull&gt; (&lt;country/regionfull&gt;) in the .NET Framework language
        /// </summary>
        DisplayName,
        /// <summary>
        /// Displays the culture name in the format "&lt;languagefull&gt; (&lt;country/regionfull&gt;) in English
        /// </summary>
        EnglishName,
        /// <summary>
        /// Displays the culture identifier
        /// </summary>
        Lcid,
        /// <summary>
        /// Displays the culture name in the format "&lt;languagecode2&gt; (&lt;country/regioncode2&gt;)
        /// </summary>
        Name,
        /// <summary>
        /// Displays the culture name in the format "&lt;languagefull&gt; (&lt;country/regionfull&gt;) in the language that the culture is set to display
        /// </summary>
        NativeName,
        /// <summary>
        /// Displays the IS0 639-1 two letter code
        /// </summary>
        TwoLetterIsoCode,
        /// <summary>
        /// Displays the ISO 629-2 three letter code "&lt;languagefull&gt; (&lt;country/regionfull&gt;)
        /// </summary>
        ThreeLetterIsoCode
    }

    /// <summary>
    /// Localization class support localization in system.
    /// </summary>
    /// <remarks>
    /// <para>As DNN is used in more and more countries it is very important to provide modules with 
    /// good support for international users. Otherwise we are limiting our potential user base to 
    /// that using English as their base language.</para>
    /// <para>
    /// You can store the muti language content in resource files and use the api below to get localization content.
    /// Resouces files named as: Control(Page)Name + Extension (.aspx/.ascx ) + Language + ".resx"
    /// e.g: Installwizard.aspx.de-DE.resx
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// pageCreationProgressArea.Localization.Total = Localization.GetString("TotalLanguages", LocalResourceFile);
    /// pageCreationProgressArea.Localization.TotalFiles = Localization.GetString("TotalPages", LocalResourceFile);
    /// pageCreationProgressArea.Localization.Uploaded = Localization.GetString("TotalProgress", LocalResourceFile);
    /// pageCreationProgressArea.Localization.UploadedFiles = Localization.GetString("Progress", LocalResourceFile);
    /// pageCreationProgressArea.Localization.CurrentFileName = Localization.GetString("Processing", LocalResourceFile);
    /// </code>
    /// </example>
    public class Localization
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Localization));
        #region Private Members

        private static string _defaultKeyName = "resourcekey";
        //private static readonly ILocaleController LocaleController.Instance = LocaleController.Instance;
        //private static readonly ILocalizationProvider _localizationProvider = LocalizationProvider.Instance;
        private static Nullable<Boolean> _showMissingKeys;

        #endregion

        #region Public Shared Properties

        public static string ApplicationResourceDirectory
        {
            get
            {
                return "~/App_GlobalResources";
            }
        }

        public static string ExceptionsResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/Exceptions.resx";
            }
        }

        public static string GlobalResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/GlobalResources.resx";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The KeyName property returns and caches the name of the key attribute used to lookup resources.
        /// This can be configured by setting ResourceManagerKey property in the web.config file. The default value for this property
        /// is 'key'.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static string KeyName
        {
            get
            {
                return _defaultKeyName;
            }
            set
            {
                _defaultKeyName = value;
                if (String.IsNullOrEmpty(_defaultKeyName))
                {
                    _defaultKeyName = "resourcekey";
                }
            }
        }

        public static string LocalResourceDirectory
        {
            get
            {
                return "App_LocalResources";
            }
        }

        public static string LocalSharedResourceFile
        {
            get
            {
                return "SharedResources.resx";
            }
        }

        public static string SharedResourceFile
        {
            get
            {
                return ApplicationResourceDirectory + "/SharedResources.resx";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ShowMissingKeys property returns the web.config setting that determines
        /// whether to render a visual indicator that a key is missing
        /// is 'key'.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	11/20/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static bool ShowMissingKeys
        {
            get
            {
                if (_showMissingKeys == null)
                {
                    if (Config.GetSetting("ShowMissingKeys") == null)
                    {
                        _showMissingKeys = false;
                    }
                    else
                    {
                        _showMissingKeys = bool.Parse(Config.GetSetting("ShowMissingKeys".ToLower()));
                    }
                }

                return _showMissingKeys.Value;
            }
        }

        public static string SupportedLocalesFile
        {
            get
            {
                return ApplicationResourceDirectory + "/Locales.xml";
            }
        }

        public static string SystemLocale
        {
            get
            {
                return "en-US";
            }
        }

        public static string SystemTimeZone
        {
            get
            {
                return "Pacific Standard Time";
            }
        }

        #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The CurrentCulture returns the current Culture being used
        /// is 'key'.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string CurrentCulture
        {
            get
            {
                //_CurrentCulture
                return Thread.CurrentThread.CurrentCulture.ToString();
            }
        }

        /// <summary>
        /// The CurrentUICulture for the Thread
        /// </summary>
        public string CurrentUICulture
        {
            // _CurrentCulture
            get
            {
                return Thread.CurrentThread.CurrentUICulture.ToString();
            }
        }

        #endregion

        #region Private Shared Methods

        private static void LocalizeDataControlField(DataControlField controlField, string resourceFile)
        {
            string localizedText;

            //Localize Header Text
            if (!string.IsNullOrEmpty(controlField.HeaderText))
            {
                localizedText = GetString((controlField.HeaderText + ".Header"), resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    controlField.HeaderText = localizedText;
                    controlField.AccessibleHeaderText = controlField.HeaderText;
                }
            }
            if (controlField is TemplateField)
            {
                //do nothing
            }
            else if (controlField is ButtonField)
            {
                var button = (ButtonField)controlField;
                localizedText = GetString(button.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    button.Text = localizedText;
                }
            }
            else if (controlField is CheckBoxField)
            {
                var checkbox = (CheckBoxField)controlField;
                localizedText = GetString(checkbox.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    checkbox.Text = localizedText;
                }
            }
            else if (controlField is CommandField)
            {
                var commands = (CommandField)controlField;
                localizedText = GetString(commands.CancelText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.CancelText = localizedText;
                }
                localizedText = GetString(commands.DeleteText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.DeleteText = localizedText;
                }
                localizedText = GetString(commands.EditText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.EditText = localizedText;
                }
                localizedText = GetString(commands.InsertText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.InsertText = localizedText;
                }
                localizedText = GetString(commands.NewText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.NewText = localizedText;
                }
                localizedText = GetString(commands.SelectText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.SelectText = localizedText;
                }
                localizedText = GetString(commands.UpdateText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    commands.UpdateText = localizedText;
                }
            }
            else if (controlField is HyperLinkField)
            {
                var link = (HyperLinkField)controlField;
                localizedText = GetString(link.Text, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    link.Text = localizedText;
                }
            }
            else if (controlField is ImageField)
            {
                var image = (ImageField)controlField;
                localizedText = GetString(image.AlternateText, resourceFile);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    image.AlternateText = localizedText;
                }
            }
        }

        #endregion

        #region Public Methods

        public static int ActiveLanguagesByPortalID(int portalID)
        {
            //Default to 1 (maybe called during portal creation before languages are enabled for portal)

            int count = 1;
            Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalID);
            if (locales != null)
            {
                count = locales.Count;
            }
            return count;
        }

        public static void AddLanguageToPortal(int portalID, int languageID, bool clearCache)
        {

            // try to get valid locale reference
            var newLocale = LocaleController.Instance.GetLocale(languageID);

            // we can only add a valid locale
            if (newLocale != null)
            {
                // check if locale has not been added to portal already
                var portalLocale = LocaleController.Instance.GetLocale(portalID, newLocale.Code);

                // locale needs to be added
                if (portalLocale == null)
                {
                    //We need to add a translator role for the language
                    bool contentLocalizationEnabled = PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", portalID, false);
                    if (contentLocalizationEnabled)
                    {
                        //Create new Translator Role
                        AddTranslatorRole(portalID, newLocale);
                    }

                    DataProvider.Instance().AddPortalLanguage(portalID, languageID, false, UserController.GetCurrentUserInfo().UserID);
                    string cacheKey = string.Format(DataCache.LocalesCacheKey, portalID);
                    DataCache.RemoveCache(cacheKey);

                    var objEventLog = new EventLogController();
                    objEventLog.AddLog("portalID/languageID",
                                       portalID + "/" + languageID,
                                       PortalController.GetCurrentPortalSettings(),
                                       UserController.GetCurrentUserInfo().UserID,
                                       EventLogController.EventLogType.LANGUAGETOPORTAL_CREATED);

                    var portalInfo = new PortalController().GetPortal(portalID);
                    if (portalInfo != null && newLocale.Code != portalInfo.DefaultLanguage)
                    {
                        // check to see if this is the first extra language being added to the portal
                        var portalLocales = LocaleController.Instance.GetLocales(portalID);
                        var firstExtraLanguage = (portalLocales != null) && portalLocales.Count == 2;

                        if (firstExtraLanguage)
                        {
                            AddLanguageHttpAlias(portalID, LocaleController.Instance.GetLocale(portalID, portalInfo.DefaultLanguage));
                        }
                        AddLanguageHttpAlias(portalID, newLocale);
                    }

                    if (clearCache)
                    {
                        DataCache.ClearPortalCache(portalID, false);
                    }
                }
            }
        }


        private static void AddLanguageHttpAlias(int portalId, Locale locale)
        {
            if (Config.GetFriendlyUrlProvider() == "advanced")
            {
                // create new HTTPAlias for language
                var portalInfo = new PortalController().GetPortal(portalId);
                PortalAliasInfo currentAlias = null;
                string httpAlias = null;

                var portalAliasses = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
                var portalAliasInfos = portalAliasses as IList<PortalAliasInfo> ?? portalAliasses.ToList();
                if (portalAliasses != null && portalAliasInfos.Any())
                {
                    currentAlias = portalAliasInfos.First();
                    httpAlias = currentAlias.HTTPAlias;
                }

                if (currentAlias != null && !string.IsNullOrEmpty(httpAlias) && portalInfo != null)
                {

                    if (!string.IsNullOrEmpty(currentAlias.CultureCode))
                    {
                        // the portal alias is culture specific
                        if (currentAlias.CultureCode == portalInfo.CultureCode)
                        {
                            // remove the culture from the alias
                            httpAlias = httpAlias.Substring(0, httpAlias.LastIndexOf("/", StringComparison.Ordinal));
                        }
                    }

                    var alias = GetValidLanguageURL(portalId, httpAlias, locale.Code.ToLowerInvariant());
                    if (!String.IsNullOrEmpty(alias))
                    {
                        var newAlias = new PortalAliasInfo(currentAlias)
                            {
                                IsPrimary = true,
                                CultureCode = locale.Code,
                                HTTPAlias = GetValidLanguageURL(portalId, httpAlias, locale.Code.ToLowerInvariant())
                            };

                        TestablePortalAliasController.Instance.AddPortalAlias(newAlias);
                    }
                }
            }
        }

        private static string GetValidLanguageURL(int portalId, string httpAlias, string locale)
        {
            string alias;

            bool isValid;
            int counter = 0;
            do
            {
                string modifiedLocale = locale;
                if (counter > 0)
                {
                    modifiedLocale += counter.ToString(CultureInfo.InvariantCulture);
                }

                alias = String.Format("{0}/{1}", httpAlias, modifiedLocale);

                var tab = new TabController().GetTabByName(modifiedLocale, portalId);
                isValid = (tab == null);

                if (isValid)
                {
                    var user = UserController.GetUserByVanityUrl(portalId, modifiedLocale);
                    isValid = (user == null);
                }

                if (isValid)
                {
                    PortalAliasInfo existingAlias;
                    TestablePortalAliasController.Instance.GetPortalAliases().TryGetValue(alias, out existingAlias);

                    isValid = (existingAlias == null);
                }

                if (isValid)
                {
                    isValid = PortalAliasController.ValidateAlias(alias, false);
                }

                counter++;
            } while (!isValid);

            return alias;
        }

        public static void AddLanguagesToPortal(int portalID)
        {
            foreach (Locale language in LocaleController.Instance.GetLocales(Null.NullInteger).Values)
            {
                //Add Portal/Language to PortalLanguages
                AddLanguageToPortal(portalID, language.LanguageId, false);
            }
            DataCache.RemoveCache(String.Format(DataCache.LocalesCacheKey, portalID));
        }

        public static void AddLanguageToPortals(int languageID)
        {
            var controller = new PortalController();
            foreach (PortalInfo portal in controller.GetPortals())
            {
                //Add Portal/Language to PortalLanguages
                AddLanguageToPortal(portal.PortalID, languageID, false);

                DataCache.RemoveCache(String.Format(DataCache.LocalesCacheKey, portal.PortalID));
            }
        }

        public static void AddTranslatorRole(int portalID, Locale language)
        {
            //Create new Translator Role
            string roleName = string.Format("Translator ({0})", language.Code);
            RoleInfo role = TestableRoleController.Instance.GetRole(portalID, r => r.RoleName == roleName);

            if (role == null)
            {
                role = new RoleInfo();
                role.RoleGroupID = Null.NullInteger;
                role.PortalID = portalID;
                role.RoleName = roleName;
                role.Description = string.Format("A role for {0} translators", language.EnglishName);
                role.SecurityMode = SecurityMode.SecurityRole;
                role.Status = RoleStatus.Approved;
                TestableRoleController.Instance.AddRole(role);
            }

            string roles = string.Format("Administrators;{0}", string.Format("Translator ({0})", language.Code));

            PortalController.UpdatePortalSetting(portalID, string.Format("DefaultTranslatorRoles-{0}", language.Code), roles);
        }

        /// <summary>
        /// Converts old TimeZoneOffset to new TimeZoneInfo. 
        /// </summary>
        /// <param name="timeZoneOffsetInMinutes">An offset in minutes, e.g. -480 (-8 times 60) for Pasicif Time Zone</param>        
        /// <returns>TimeZoneInfo is returned if timeZoneOffsetInMinutes is valid, otherwise TimeZoneInfo.Local is returned.</returns>
        /// <remarks>Initial mapping is based on hard-coded rules. These rules are hard-coded from old standard TimeZones.xml data.
        /// When offset is not found hard-coded mapping, a lookup is performed in timezones defined in the system. The first found entry is returned.
        /// When mapping is not found, a default TimeZoneInfo.Local us returned.</remarks>
        public static TimeZoneInfo ConvertLegacyTimeZoneOffsetToTimeZoneInfo(int timeZoneOffsetInMinutes)
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            //lookup existing mapping
            switch (timeZoneOffsetInMinutes)
            {
                case -720:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Dateline Standard Time");
                    break;
                case -660:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Samoa Standard Time");
                    break;
                case -600:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time");
                    break;
                case -540:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time");
                    break;
                case -480:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    break;
                case -420:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                    break;
                case -360:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                    break;
                case -300:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    break;
                case -240:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
                    break;
                case -210:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Newfoundland Standard Time");
                    break;
                case -180:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                    break;
                case -120:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Mid-Atlantic Standard Time");
                    break;
                case -60:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Cape Verde Standard Time");
                    break;
                case 0:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    break;
                case 60:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    break;
                case 120:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GTB Standard Time");
                    break;
                case 180:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                    break;
                case 210:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                    break;
                case 240:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
                    break;
                case 270:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Afghanistan Standard Time");
                    break;
                case 300:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                    break;
                case 330:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    break;
                case 345:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");
                    break;
                case 360:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Asia Standard Time");
                    break;
                case 390:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
                    break;
                case 420:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    break;
                case 480:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    break;
                case 540:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                    break;
                case 570:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Cen. Australia Standard Time");
                    break;
                case 600:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                    break;
                case 660:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Magadan Standard Time");
                    break;
                case 720:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                    break;
                case 780:
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tonga Standard Time");
                    break;
                default:
                    foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
                    {
                        if (timeZone.BaseUtcOffset.TotalMinutes == timeZoneOffsetInMinutes)
                        {
                            timeZoneInfo = timeZone;
                            break;
                        }
                    }
                    break;
            }

            return timeZoneInfo;
        }

        public static void DeleteLanguage(Locale language)
        {
            //remove languages from all portals
            RemoveLanguageFromPortals(language.LanguageId);

            DataProvider.Instance().DeleteLanguage(language.LanguageId);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(language, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.LANGUAGE_DELETED);
            DataCache.ClearHostCache(true);
        }

        
        public static string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes, string fallback)
        {
            return TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(cultureCodes, fallback);
        }

        public static string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes)
        {
            return TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(cultureCodes);
        }

        #region GetExceptionMessage

        public static string GetExceptionMessage(string key, string defaultValue)
        {
            if (HttpContext.Current == null)
            {
                return defaultValue;
            }
            return GetString(key, ExceptionsResourceFile);
        }

        public static string GetExceptionMessage(string key, string defaultValue, params object[] @params)
        {
            if (HttpContext.Current == null)
            {
                return string.Format(defaultValue, @params);
            }
            var content = GetString(key, ExceptionsResourceFile);
            return string.Format(String.IsNullOrEmpty(content) ? defaultValue : GetString(key, ExceptionsResourceFile), @params);
        }

        #endregion

        public string GetFixedCurrency(decimal expression, string culture, int numDigitsAfterDecimal)
        {
            string oldCurrentCulture = CurrentUICulture;
            var newCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = newCulture;
            string currencyStr = expression.ToString(newCulture.NumberFormat.CurrencySymbol);
            var oldCulture = new CultureInfo(oldCurrentCulture);
            Thread.CurrentThread.CurrentUICulture = oldCulture;
            return currencyStr;
        }

        public string GetFixedDate(DateTime expression, string culture)
        {
            string oldCurrentCulture = CurrentUICulture;
            var newCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = newCulture;
            string dateStr = expression.ToString(newCulture.DateTimeFormat.FullDateTimePattern);
            var oldCulture = new CultureInfo(oldCurrentCulture);
            Thread.CurrentThread.CurrentUICulture = oldCulture;
            return dateStr;
        }

        public static string GetLanguageDisplayMode(int portalId)
        {
            string viewTypePersonalizationKey = "ViewType" + portalId;
            string viewType = Convert.ToString(Personalization.Personalization.GetProfile("LanguageDisplayMode", viewTypePersonalizationKey));
            if (string.IsNullOrEmpty(viewType))
            {
                viewType = "NATIVE";
            }
            return viewType;
        }

        public static string GetLocaleName(string code, CultureDropDownTypes displayType)
        {
            string name;

            // Create a CultureInfo class based on culture
            CultureInfo info = CultureInfo.CreateSpecificCulture(code);

            // Based on the display type desired by the user, select the correct property
            switch (displayType)
            {
                case CultureDropDownTypes.EnglishName:
                    name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.EnglishName);
                    break;
                case CultureDropDownTypes.Lcid:
                    name = info.LCID.ToString();
                    break;
                case CultureDropDownTypes.Name:
                    name = info.Name;
                    break;
                case CultureDropDownTypes.NativeName:
                    name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(info.NativeName);
                    break;
                case CultureDropDownTypes.TwoLetterIsoCode:
                    name = info.TwoLetterISOLanguageName;
                    break;
                case CultureDropDownTypes.ThreeLetterIsoCode:
                    name = info.ThreeLetterISOLanguageName;
                    break;
                default:
                    name = info.DisplayName;
                    break;
            }

            return name;
        }

		#region Language detection
		/// <summary>
		/// Detects the current language for the request.
		/// The order in which the language is being detect is:
		///		1. QueryString
		///		2. Cookie
		///		3. User profile (if request is authenticated)
		///		4. Browser preference (if portal has this option enabled)
		///		5. Portal default
		///		6. System default (en-US)
		///	At any point, if a valid language is detected nothing else should be done
		/// </summary>
		/// <param name="portalSettings">Current PortalSettings</param>
		/// <returns>A valid CultureInfo</returns>
		public static CultureInfo GetPageLocale(PortalSettings portalSettings)
		{
			CultureInfo pageCulture = null;

			// 1. querystring
			if (portalSettings != null)
				pageCulture = GetCultureFromQs(portalSettings);

			// 2. cookie
			if (portalSettings != null && pageCulture == null)
				pageCulture = GetCultureFromCookie(portalSettings);

			// 3. user preference
			if (portalSettings != null && pageCulture == null)
				pageCulture = GetCultureFromProfile(portalSettings);

			// 4. browser
			if (portalSettings != null && pageCulture == null)
				pageCulture = GetCultureFromBrowser(portalSettings);

			// 5. portal default
			if (portalSettings != null && pageCulture == null)
				pageCulture = GetCultureFromPortal(portalSettings);

			// 6. system default
			if (pageCulture == null)
				pageCulture = new CultureInfo(SystemLocale);

			// finally set the cookie
			SetLanguage(pageCulture.Name);
			return pageCulture;
		}

		/// <summary>
		/// Tries to get a valid language from the querystring
		/// </summary>
		/// <param name="portalSettings">Current PortalSettings</param>
		/// <returns>A valid CultureInfo if any is found</returns>
		private static CultureInfo GetCultureFromQs(PortalSettings portalSettings)
		{
			if (HttpContext.Current == null || HttpContext.Current.Request["language"] == null)
				return null;

			string language = HttpContext.Current.Request["language"];
			CultureInfo culture = GetCultureFromString(portalSettings.PortalId, language);
			return culture;
		}

		/// <summary>
		/// Tries to get a valid language from the cookie
		/// </summary>
		/// <param name="portalSettings">Current PortalSettings</param>
		/// <returns>A valid CultureInfo if any is found</returns>
		private static CultureInfo GetCultureFromCookie(PortalSettings portalSettings)
		{
			CultureInfo culture;
			if (HttpContext.Current == null || HttpContext.Current.Request.Cookies["language"] == null)
				return null;

			string language = HttpContext.Current.Request.Cookies["language"].Value;
			culture = GetCultureFromString(portalSettings.PortalId, language);
			return culture;
		}

		/// <summary>
		/// Tries to get a valid language from the user profile
		/// </summary>
		/// <param name="portalSettings">Current PortalSettings</param>
		/// <returns>A valid CultureInfo if any is found</returns>
		private static CultureInfo GetCultureFromProfile(PortalSettings portalSettings)
		{
			UserInfo objUserInfo = UserController.GetCurrentUserInfo();

			if (HttpContext.Current == null || !HttpContext.Current.Request.IsAuthenticated || objUserInfo.UserID == -1)
				return null;

			string language = objUserInfo.Profile.PreferredLocale;
			CultureInfo culture = GetCultureFromString(portalSettings.PortalId, language);
			return culture;
		}

		/// <summary>
		/// Tries to get a valid language from the browser preferences if the portal has the setting 
		/// to use browser languages enabled.
		/// </summary>
		/// <param name="portalSettings">Current PortalSettings</param>
		/// <returns>A valid CultureInfo if any is found</returns>
		private static CultureInfo GetCultureFromBrowser(PortalSettings portalSettings)
		{
			if (!portalSettings.EnableBrowserLanguage)
				return null;
			else
				return GetBrowserCulture(portalSettings.PortalId);
		}

		/// <summary>
		/// Tries to get a valid language from the portal default preferences
		/// </summary>
		/// <param name="portalSettings">Current PortalSettings</param>
		/// <returns>A valid CultureInfo if any is found</returns>
		private static CultureInfo GetCultureFromPortal(PortalSettings portalSettings)
		{
			CultureInfo culture = null;
			if (!String.IsNullOrEmpty(portalSettings.DefaultLanguage))
			{
				// As the portal default language can never be disabled, we know this language is available and enabled
				culture = new CultureInfo(portalSettings.DefaultLanguage);
			}
			else
			{
				// Get the first enabled locale on the portal
				Dictionary<string, Locale> enabledLocales = new Dictionary<string, Locale>();
				if (portalSettings.PortalId > Null.NullInteger)
					enabledLocales = LocaleController.Instance.GetLocales(portalSettings.PortalId);

				if (enabledLocales.Count > 0)
				{
					foreach (string localeCode in enabledLocales.Keys)
					{
						culture = new CultureInfo(localeCode);
						break;
					}
				}
			}
			return culture;
		}

		/// <summary>
		/// Tries to get a valid language from the browser preferences
		/// </summary>
		/// <param name="portalId">Id of the current portal</param>
		/// <returns>A valid CultureInfo if any is found</returns>
		public static CultureInfo GetBrowserCulture(int portalId)
		{
            if (HttpContext.Current == null || HttpContext.Current.Request == null || HttpContext.Current.Request.UserLanguages == null)
				return null;

			CultureInfo culture = null;
			foreach (string userLang in HttpContext.Current.Request.UserLanguages)
			{
				//split userlanguage by ";"... all but the first language will contain a preferrence index eg. ;q=.5
				string language = userLang.Split(';')[0];
				culture = GetCultureFromString(portalId, language);
				if (culture != null)
					break;
			}
			return culture;
		}

		/// <summary>
		/// Parses the language parameter into a valid and enabled language in the current portal.
		/// If an exact match is not found (language-region), it will try to find a match for the language only.
		/// Ex: requested locale is "en-GB", requested language is "en", enabled locale is "en-US", so "en" is a match for "en-US".
		/// </summary>
		/// <param name="portalId">Id of current portal</param>
		/// <param name="language">Language to be parsed</param>
		/// <returns>A valid and enabled CultureInfo that matches the language passed if any.</returns>
		private static CultureInfo GetCultureFromString(int portalId, string language)
		{
			CultureInfo culture = null;
			if (!String.IsNullOrEmpty(language))
			{
				if (LocaleController.Instance.IsEnabled(ref language, portalId))
					culture = new CultureInfo(language);
				else
				{
					string preferredLanguage = language.Split('-')[0];

					Dictionary<string, Locale> enabledLocales = new Dictionary<string, Locale>();
					if (portalId > Null.NullInteger)
						enabledLocales = LocaleController.Instance.GetLocales(portalId);

					foreach (string localeCode in enabledLocales.Keys)
					{
						if (localeCode.Split('-')[0] == preferredLanguage.Split('-')[0])
						{
							culture = new CultureInfo(localeCode);
							break;
						}
					}
				}
			}
			return culture;
		}
		#endregion

        public static string GetResourceFileName(string resourceFileName, string language, string mode, int portalId)
        {
            if (!resourceFileName.EndsWith(".resx"))
            {
                resourceFileName += ".resx";
            }
            if (language != SystemLocale)
            {
                if (resourceFileName.ToLowerInvariant().EndsWith(".en-us.resx"))
                {
                    resourceFileName = resourceFileName.Substring(0, resourceFileName.Length - 11) + "." + language + ".resx";
                }
                else
                {
                    resourceFileName = resourceFileName.Substring(0, resourceFileName.Length - 5) + "." + language + ".resx";
                }
            }
            if (mode == "Host")
            {
                resourceFileName = resourceFileName.Substring(0, resourceFileName.Length - 5) + "." + "Host.resx";
            }
            else if (mode == "Portal")
            {
                resourceFileName = resourceFileName.Substring(0, resourceFileName.Length - 5) + "." + "Portal-" + portalId + ".resx";
            }
            return resourceFileName;
        }

        public static string GetResourceFile(Control control, string fileName)
        {
            return control.TemplateSourceDirectory + "/" + LocalResourceDirectory + "/" + fileName;
        }

        #region GetString

        public static string GetString(string key, Control ctrl)
        {
            //We need to find the parent module
            Control parentControl = ctrl.Parent;
            string localizedText;
            var moduleControl = parentControl as IModuleControl;
            if (moduleControl == null)
            {
                PropertyInfo pi = parentControl.GetType().GetProperty("LocalResourceFile");
                if (pi != null)
                {
                    //If control has a LocalResourceFile property use this
                    localizedText = GetString(key, pi.GetValue(parentControl, null).ToString());
                }
                else
                {
                    //Drill up to the next level 
                    localizedText = GetString(key, parentControl);
                }
            }
            else
            {
                //We are at the Module Level so return key
                //Get Resource File Root from Parents LocalResourceFile Property
                localizedText = GetString(key, moduleControl.LocalResourceFile);
            }
            return localizedText;
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resource key
        /// </summary>
        /// <param name="key">The resource key to find</param>
        /// <returns>The localized Text</returns>
        /// <history>
        /// 	[cnurse]	10/06/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key)
        {
            return GetString(key, null, PortalController.GetCurrentPortalSettings(), null, false);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="portalSettings">The current portals Portal Settings</param>
        /// <returns>The localized Text</returns>
        /// <history>
        /// 	[cnurse]	10/06/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key, PortalSettings portalSettings)
        {
            return LocalizationProvider.Instance.GetString(key, null, null, portalSettings);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <param name="disableShowMissingKeys">Disable to show missing key.</param>
        /// <returns>The localized Text</returns>
        /// <history>
        /// 	[cnurse]	10/06/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key, string resourceFileRoot, bool disableShowMissingKeys)
        {
            return GetString(key, resourceFileRoot, PortalController.GetCurrentPortalSettings(), null, disableShowMissingKeys);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Resource File Name.</param>
        /// <returns>The localized Text</returns>
        /// <history>
        /// 	[cnurse]	10/06/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key, string resourceFileRoot)
        {
            return LocalizationProvider.Instance.GetString(key, resourceFileRoot);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <param name="language">A specific language to lookup the string</param>
        /// <returns>The localized Text</returns>
        /// <history>
        /// 	[cnurse]	10/06/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key, string resourceFileRoot, string language)
        {
            return LocalizationProvider.Instance.GetString(key, resourceFileRoot, language);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <param name="portalSettings">The current portals Portal Settings</param>
        /// <param name="language">A specific language to lookup the string</param>
        /// <returns>The localized Text</returns>
        /// <history>
        /// 	[cnurse]	10/06/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key, string resourceFileRoot, PortalSettings portalSettings, string language)
        {
            return GetString(key, resourceFileRoot, portalSettings, language, false);
        }

        /// -----------------------------------------------------------------------------
        /// <overloads>One of six overloads</overloads>
        /// <summary>
        /// GetString gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <param name="portalSettings">The current portals Portal Settings</param>
        /// <param name="language">A specific language to lookup the string</param>
        /// <param name="disableShowMissingKeys">Disables the show missing keys flag</param>
        /// <returns>The localized Text</returns>
        /// <history>
        /// 	[cnurse]	10/06/2004	Documented
        ///     [cnurse]    01/30/2008  Refactored to use Dictionaries and to cahe the portal and host 
        ///                             customisations and language versions separately
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetString(string key, string resourceFileRoot, PortalSettings portalSettings, string language, bool disableShowMissingKeys)
        {
            return LocalizationProvider.Instance.GetString(key, resourceFileRoot, language, portalSettings, disableShowMissingKeys);
        }

        #endregion

        #region GetStringUrl

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetStringUrl gets the localized string corresponding to the resourcekey
        /// </summary>
        /// <param name="key">The resourcekey to find</param>
        /// <param name="resourceFileRoot">The Local Resource root</param>
        /// <returns>The localized Text</returns>
        /// <remarks>
        /// This function should be used to retrieve strings to be used on URLs.
        /// It is the same as <see cref="GetString(string, string)">GetString(name,ResourceFileRoot)</see> method
        /// but it disables the ShowMissingKey flag, so even it testing scenarios, the correct string
        /// is returned
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static string GetStringUrl(string key, string resourceFileRoot)
        {
            return GetString(key, resourceFileRoot, PortalController.GetCurrentPortalSettings(), null, true);
        }

        #endregion

        #region GetSafeJSString
        
        /// <summary>
        /// this function will escape reserved character fields to their "safe" javascript equivalents
        /// </summary>
        /// <param name="unsafeString">The string to be parsed for unsafe characters</param>
        /// <returns>the string that is safe to use in a javascript function</returns>
        public static string GetSafeJSString(string unsafeString)
        {
            return !string.IsNullOrEmpty(unsafeString) && unsafeString.Length > 0 ? Regex.Replace(unsafeString, "(['\"\\\\])", "\\$1") : unsafeString;
        }

        /// <summary>
        /// this function will escape reserved character fields to their "safe" javascript equivalents
        /// </summary>
        /// <param name="key">localization key</param>
        /// <param name="resourceFileRoot">file for localization key</param>
        /// <returns>the string that is safe to use in a javascript function</returns>
        public static string GetSafeJSString(string key, string resourceFileRoot)
        {
            var unsafeString = GetString(key, resourceFileRoot);
            return GetSafeJSString(unsafeString);
        }

        #endregion

        #region Get System Message

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage.
        /// </summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// - All fields from HostSettings table in the form of: [Host:<b>field</b>]
        /// - All properties defined in <see cref="T:DotNetNuke.PortalInfo" /> in the form of: [Portal:<b>property</b>]
        /// - [Portal:URL]: The base URL for the portal
        /// - All properties defined in <see cref="T:DotNetNuke.UserInfo" /> in the form of: [User:<b>property</b>]
        /// - All values stored in the user profile in the form of: [Profile:<b>key</b>]
        /// - [User:VerificationCode]: User verification code for verified registrations
        /// - [Date:Current]: Current date
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName)
        {
            return GetSystemMessage(null, portalSettings, messageName, null, GlobalResourceFile, null);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage.
        /// </summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// - All fields from HostSettings table in the form of: [Host:<b>field</b>]
        /// - All properties defined in <see cref="T:DotNetNuke.PortalInfo" /> in the form of: [Portal:<b>property</b>]
        /// - [Portal:URL]: The base URL for the portal
        /// - All properties defined in <see cref="T:DotNetNuke.UserInfo" /> in the form of: [User:<b>property</b>]
        /// - All values stored in the user profile in the form of: [Profile:<b>key</b>]
        /// - [User:VerificationCode]: User verification code for verified registrations
        /// - [Date:Current]: Current date
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, UserInfo userInfo)
        {
            return GetSystemMessage(null, portalSettings, messageName, userInfo, GlobalResourceFile, null);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///         /// Gets a SystemMessage.
        /// </summary>
        /// <param name="strLanguage">A specific language to get the SystemMessage for.</param>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// - All fields from HostSettings table in the form of: [Host:<b>field</b>]
        /// - All properties defined in <see cref="T:DotNetNuke.PortalInfo" /> in the form of: [Portal:<b>property</b>]
        /// - [Portal:URL]: The base URL for the portal
        /// - All properties defined in <see cref="T:DotNetNuke.UserInfo" /> in the form of: [User:<b>property</b>]
        /// - All values stored in the user profile in the form of: [Profile:<b>key</b>]
        /// - [User:VerificationCode]: User verification code for verified registrations
        /// - [Date:Current]: Current date
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(string strLanguage, PortalSettings portalSettings, string messageName, UserInfo userInfo)
        {
            return GetSystemMessage(strLanguage, portalSettings, messageName, userInfo, GlobalResourceFile, null);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage.
        /// </summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// - All fields from HostSettings table in the form of: [Host:<b>field</b>]
        /// - All properties defined in <see cref="T:DotNetNuke.PortalInfo" /> in the form of: [Portal:<b>property</b>]
        /// - [Portal:URL]: The base URL for the portal
        /// - All properties defined in <see cref="T:DotNetNuke.UserInfo" /> in the form of: [User:<b>property</b>]
        /// - All values stored in the user profile in the form of: [Profile:<b>key</b>]
        /// - [User:VerificationCode]: User verification code for verified registrations
        /// - [Date:Current]: Current date
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, string resourceFile)
        {
            return GetSystemMessage(null, portalSettings, messageName, null, resourceFile, null);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage.
        /// </summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Supported tags:
        /// - All fields from HostSettings table in the form of: [Host:<b>field</b>]
        /// - All properties defined in <see cref="T:DotNetNuke.PortalInfo" /> in the form of: [Portal:<b>property</b>]
        /// - [Portal:URL]: The base URL for the portal
        /// - All properties defined in <see cref="T:DotNetNuke.UserInfo" /> in the form of: [User:<b>property</b>]
        /// - All values stored in the user profile in the form of: [Profile:<b>key</b>]
        /// - [User:VerificationCode]: User verification code for verified registrations
        /// - [Date:Current]: Current date
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile)
        {
            return GetSystemMessage(null, portalSettings, messageName, userInfo, resourceFile, null);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage passing extra custom parameters to personalize.
        /// </summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        /// <param name="custom">An ArrayList with replacements for custom tags.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <b>[Custom:n]</b>, where <b>n</b> is the zero based index which 
        /// will be used to find the replacement value in <b>Custom</b> parameter.
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        ///     [cnurse]    10/06/2004  Moved from SystemMessages to Localization
        ///     [DanCaron]  10/27/2004  Simplified Profile replacement, added Membership replacement
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, string resourceFile, ArrayList custom)
        {
            return GetSystemMessage(null, portalSettings, messageName, null, resourceFile, custom);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage passing extra custom parameters to personalize.
        /// </summary>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        /// <param name="custom">An ArrayList with replacements for custom tags.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <b>[Custom:n]</b>, where <b>n</b> is the zero based index which 
        /// will be used to find the replacement value in <b>Custom</b> parameter.
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        ///     [cnurse]    10/06/2004  Moved from SystemMessages to Localization
        ///     [DanCaron]  10/27/2004  Simplified Profile replacement, added Membership replacement
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile, ArrayList custom)
        {
            return GetSystemMessage(null, portalSettings, messageName, userInfo, resourceFile, custom);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage passing extra custom parameters to personalize.
        /// </summary>
        /// <param name="strLanguage">A specific language to get the SystemMessage for.</param>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        /// <param name="custom">An ArrayList with replacements for custom tags.</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <b>[Custom:n]</b>, where <b>n</b> is the zero based index which 
        /// will be used to find the replacement value in <b>Custom</b> parameter.
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        ///     [cnurse]    10/06/2004  Moved from SystemMessages to Localization
        ///     [DanCaron]  10/27/2004  Simplified Profile replacement, added Membership replacement
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(string strLanguage, PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile, ArrayList custom)
        {
            return GetSystemMessage(strLanguage, portalSettings, messageName, userInfo, resourceFile, custom, null, "", -1);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage passing extra custom parameters to personalize.
        /// </summary>
        /// <param name="strLanguage">A specific language to get the SystemMessage for.</param>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        /// <param name="custom">An ArrayList with replacements for custom tags.</param>
        /// <param name="customCaption">prefix for custom tags</param>
        /// <param name="accessingUserID">UserID of the user accessing the system message</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <b>[Custom:n]</b>, where <b>n</b> is the zero based index which 
        /// will be used to find the replacement value in <b>Custom</b> parameter.
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	05/07/2004	Documented
        ///     [cnurse]    10/06/2004  Moved from SystemMessages to Localization
        ///     [DanCaron]  10/27/2004  Simplified Profile replacement, added Membership replacement
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(string strLanguage, PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile, ArrayList custom, string customCaption,
                                              int accessingUserID)
        {
            return GetSystemMessage(strLanguage, portalSettings, messageName, userInfo, resourceFile, custom, null, customCaption, accessingUserID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a SystemMessage passing extra custom parameters to personalize.
        /// </summary>
        /// <param name="strLanguage">A specific language to get the SystemMessage for.</param>
        /// <param name="portalSettings">The portal settings for the portal to which the message will affect.</param>
        /// <param name="messageName">The message tag which identifies the SystemMessage.</param>
        /// <param name="userInfo">Reference to the user used to personalize the message.</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        /// <param name="customArray">An ArrayList with replacements for custom tags.</param>
        /// <param name="customDictionary">An IDictionary with replacements for custom tags.</param>
        /// <param name="customCaption">prefix for custom tags</param>
        /// <param name="accessingUserID">UserID of the user accessing the system message</param>
        /// <returns>The message body with all tags replaced.</returns>
        /// <remarks>
        /// Custom tags are of the form <b>[Custom:n]</b>, where <b>n</b> is the zero based index which 
        /// will be used to find the replacement value in <b>Custom</b> parameter.
        /// </remarks>
        /// <history>
        ///     [cnurse]    09/09/2009  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string GetSystemMessage(string strLanguage, PortalSettings portalSettings, string messageName, UserInfo userInfo, string resourceFile, ArrayList customArray,
                                              IDictionary customDictionary, string customCaption, int accessingUserID)
        {
            string strMessageValue = GetString(messageName, resourceFile, portalSettings, strLanguage);
            if (!String.IsNullOrEmpty(strMessageValue))
            {
                if (String.IsNullOrEmpty(customCaption))
                {
                    customCaption = "Custom";
                }
                var objTokenReplace = new TokenReplace(Scope.SystemMessages, strLanguage, portalSettings, userInfo);
                if ((accessingUserID != -1) && (userInfo != null))
                {
                    if (userInfo.UserID != accessingUserID)
                    {
                        objTokenReplace.AccessingUser = new UserController().GetUser(portalSettings.PortalId, accessingUserID);
                    }
                }
                if (customArray != null)
                {
                    strMessageValue = objTokenReplace.ReplaceEnvironmentTokens(strMessageValue, customArray, customCaption);
                }
                else
                {
                    strMessageValue = objTokenReplace.ReplaceEnvironmentTokens(strMessageValue, customDictionary, customCaption);
                }
            }
            return strMessageValue;
        }

        #endregion

        #region LoadCultureDropDownList

        /// <summary>
        ///   <para>LoadCultureDropDownList loads a DropDownList with the list of supported cultures
        ///     based on the languages defined in the supported locales file, for the current portal</para>
        /// </summary>
        /// <param name = "list">DropDownList to load</param>
        /// <param name = "displayType">Format of the culture to display. Must be one the CultureDropDownTypes values.
        ///   <see cref = "CultureDropDownTypes" /> for list of allowable values</param>
        /// <param name = "selectedValue">Name of the default culture to select</param>
        public static void LoadCultureDropDownList(DropDownList list, CultureDropDownTypes displayType, string selectedValue)
        {
            LoadCultureDropDownList(list, displayType, selectedValue, "", false);
        }

        /// <summary>
        ///   <para>LoadCultureDropDownList loads a DropDownList with the list of supported cultures
        ///     based on the languages defined in the supported locales file. </para>
        ///   <para>This overload allows us to display all installed languages. To do so, pass the value True to the Host parameter</para>
        /// </summary>
        /// <param name = "list">DropDownList to load</param>
        /// <param name = "displayType">Format of the culture to display. Must be one the CultureDropDownTypes values.
        ///   <see cref = "CultureDropDownTypes" /> for list of allowable values</param>
        /// <param name = "selectedValue">Name of the default culture to select</param>
        /// <param name = "loadHost">Boolean that defines wether or not to load host (ie. all available) locales</param>
        public static void LoadCultureDropDownList(DropDownList list, CultureDropDownTypes displayType, string selectedValue, bool loadHost)
        {
            LoadCultureDropDownList(list, displayType, selectedValue, "", loadHost);
        }

        /// <summary>
        ///   <para>LoadCultureDropDownList loads a DropDownList with the list of supported cultures
        ///     based on the languages defined in the supported locales file</para>
        ///   <para>This overload allows us to filter a language from the dropdown. To do so pass a language code to the Filter parameter</para>
        ///   <para>This overload allows us to display all installed languages. To do so, pass the value True to the Host parameter</para>
        /// </summary>
        /// <param name = "list">DropDownList to load</param>
        /// <param name = "displayType">Format of the culture to display. Must be one the CultureDropDownTypes values.
        ///   <see cref = "CultureDropDownTypes" /> for list of allowable values</param>
        /// <param name = "selectedValue">Name of the default culture to select</param>
        /// <param name = "filter">String value that allows for filtering out a specific language</param>
        /// <param name = "host">Boolean that defines wether or not to load host (ie. all available) locales</param>
        public static void LoadCultureDropDownList(DropDownList list, CultureDropDownTypes displayType, string selectedValue, string filter, bool host)
        {
            IEnumerable<ListItem> cultureListItems = LoadCultureInListItems(displayType, selectedValue, filter, host);  
            
            //add the items to the list
            foreach (var cultureItem in cultureListItems)
            {
                list.Items.Add(cultureItem);
            }

            //select the default item
            if (selectedValue != null)
            {
                ListItem item = list.Items.FindByValue(selectedValue);
                if (item != null)
                {
                    list.SelectedIndex = -1;
                    item.Selected = true;
                }
            }
        }

        /// <summary>
        /// <para>LoadCultureDropDownList loads a DropDownList with the list of supported cultures
        ///     based on the languages defined in the supported locales file</para>
        ///  <para>This overload allows us to filter a language from the dropdown. To do so pass a language code to the Filter parameter</para>
        ///   <para>This overload allows us to display all installed languages. To do so, pass the value True to the Host parameter</para>
        /// </summary>
        /// <param name="displayType"></param>
        /// <param name="selectedValue"></param>
        /// <param name="filter"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IEnumerable<ListItem> LoadCultureInListItems(CultureDropDownTypes displayType, string selectedValue, string filter, bool host)
        {
            PortalSettings objPortalSettings = PortalController.GetCurrentPortalSettings();
            Dictionary<string, Locale> enabledLanguages;
            if (host)
            {
                enabledLanguages = LocaleController.Instance.GetLocales(Null.NullInteger);
            }
            else
            {
                enabledLanguages = LocaleController.Instance.GetLocales(objPortalSettings.PortalId);
            }

            var cultureListItems = new List<ListItem>(enabledLanguages.Count);
            foreach (KeyValuePair<string, Locale> kvp in enabledLanguages)
            {
                if (kvp.Value.Code != filter)
                {
                    cultureListItems.Add(new ListItem { Value = kvp.Value.Code, Text = GetLocaleName(kvp.Value.Code, displayType) });
                }
            }

            return cultureListItems;
        }

        #endregion

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Localizes ModuleControl Titles
        /// </summary>
        /// <param name="moduleControl">ModuleControl</param>
        /// <returns>
        /// Localized control title if found
        /// </returns>
        /// <remarks>
        /// Resource keys are: ControlTitle_[key].Text
        /// Key MUST be lowercase in the resource file
        /// </remarks>
        /// <history>
        /// 	[vmasanas]	08/11/2004	Created
        ///     [cnurse]    11/28/2008  Modified Signature
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string LocalizeControlTitle(IModuleControl moduleControl)
        {
            string controlTitle = moduleControl.ModuleContext.Configuration.ModuleTitle;
            string controlKey = moduleControl.ModuleContext.Configuration.ModuleControl.ControlKey.ToLower();

            if (!string.IsNullOrEmpty(controlTitle) && !string.IsNullOrEmpty(controlKey))
            {
                controlTitle = moduleControl.ModuleContext.Configuration.ModuleControl.ControlTitle;
            }

            if (!string.IsNullOrEmpty(controlKey))
            {
                string reskey = "ControlTitle_" + moduleControl.ModuleContext.Configuration.ModuleControl.ControlKey.ToLower() + ".Text";
                string localizedvalue = GetString(reskey, moduleControl.LocalResourceFile);
                if (string.IsNullOrEmpty(localizedvalue))
                {
                    controlTitle = moduleControl.ModuleContext.Configuration.ModuleControl.ControlTitle;
                }
                else
                {
                    controlTitle = localizedvalue;
                }
            }
            return controlTitle;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LocalizeDataGrid creates localized Headers for a DataGrid
        /// </summary>
        /// <param name="grid">Grid to localize</param>
        /// <param name="resourceFile">The root name of the Resource File where the localized
        ///   text can be found</param>
        /// <history>
        /// 	[cnurse]	9/10/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void LocalizeDataGrid(ref DataGrid grid, string resourceFile)
        {
            string localizedText;
            foreach (DataGridColumn col in grid.Columns)
            {
                //Localize Header Text
                if (!string.IsNullOrEmpty(col.HeaderText))
                {
                    localizedText = GetString(col.HeaderText + ".Header", resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        col.HeaderText = localizedText;
                    }
                }
                if (col is EditCommandColumn)
                {
                    var editCol = (EditCommandColumn)col;

                    //Edit Text - maintained for backward compatibility
                    localizedText = GetString(editCol.EditText + ".EditText", resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        editCol.EditText = localizedText;
                    }

                    //Edit Text
                    localizedText = GetString(editCol.EditText, resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        editCol.EditText = localizedText;
                    }

                    //Cancel Text
                    localizedText = GetString(editCol.CancelText, resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        editCol.CancelText = localizedText;
                    }

                    //Update Text
                    localizedText = GetString(editCol.UpdateText, resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        editCol.UpdateText = localizedText;
                    }
                }
                else if (col is ButtonColumn)
                {
                    var buttonCol = (ButtonColumn)col;

                    //Edit Text
                    localizedText = GetString(buttonCol.Text, resourceFile);
                    if (!String.IsNullOrEmpty(localizedText))
                    {
                        buttonCol.Text = localizedText;
                    }
                }
            }
        }

        /// <summary>
        /// Localizes headers and fields on a DetailsView control
        /// </summary>
        /// <param name="detailsView"></param>
        /// <param name="resourceFile">The root name of the resource file where the localized
        ///  texts can be found</param>
        /// <remarks></remarks>
        public static void LocalizeDetailsView(ref DetailsView detailsView, string resourceFile)
        {
            foreach (DataControlField field in detailsView.Fields)
            {
                LocalizeDataControlField(field, resourceFile);
            }
        }

        /// <summary>
        /// Localizes headers and fields on a GridView control
        /// </summary>
        /// <param name="gridView">Grid to localize</param>
        /// <param name="resourceFile">The root name of the resource file where the localized
        ///  texts can be found</param>
        /// <remarks></remarks>
        public static void LocalizeGridView(ref GridView gridView, string resourceFile)
        {
            foreach (DataControlField column in gridView.Columns)
            {
                LocalizeDataControlField(column, resourceFile);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Localizes the "Built In" Roles
        /// </summary>
        /// <remarks>
        /// Localizes:
        /// -DesktopTabs
        /// -BreadCrumbs
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/01/2005	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string LocalizeRole(string role)
        {
            string localRole;
            switch (role)
            {
                case Globals.glbRoleAllUsersName:
                case Globals.glbRoleSuperUserName:
                case Globals.glbRoleUnauthUserName:
                    string roleKey = role.Replace(" ", "");
                    localRole = GetString(roleKey);
                    break;
                default:
                    localRole = role;
                    break;
            }
            return localRole;
        }

        public static void RemoveLanguageFromPortal(int portalID, int languageID)
        {
            //Remove Translator Role from Portal
            Locale language = LocaleController.Instance.GetLocale(languageID);
            if (language != null)
            {
                if (Config.GetFriendlyUrlProvider() == "advanced")
                {
                    // only do this with Advanced URL Management

                    var portalInfo = new PortalController().GetPortal(portalID);
                    if (portalInfo != null)
                    {
                        // check to see if this is the last extra language being added to the portal
                        var lastLanguage = LocaleController.Instance.GetLocales(portalID).Count == 2;

                        var portalAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portalID);
                        foreach (var portalAliasInfo in portalAliases)
                        {
                            if (portalAliasInfo.CultureCode == language.Code)
                            {
                                TestablePortalAliasController.Instance.DeletePortalAlias(portalAliasInfo);
                            }

                            if (lastLanguage && portalAliasInfo.CultureCode == portalInfo.DefaultLanguage)
                            {
                                TestablePortalAliasController.Instance.DeletePortalAlias(portalAliasInfo);
                            }
                        }
                    }
                }

                //Get Translator Role
                string roleName = string.Format("Translator ({0})", language.Code);
                RoleInfo role = TestableRoleController.Instance.GetRole(portalID, r => r.RoleName == roleName);

                if (role != null)
                {
                    TestableRoleController.Instance.DeleteRole(role);
                }

                DataProvider.Instance().DeletePortalLanguages(portalID, languageID);
                var objEventLog = new EventLogController();
                objEventLog.AddLog("portalID/languageID",
                                   portalID + "/" + languageID,
                                   PortalController.GetCurrentPortalSettings(),
                                   UserController.GetCurrentUserInfo().UserID,
                                   EventLogController.EventLogType.LANGUAGETOPORTAL_DELETED);

                DataCache.ClearPortalCache(portalID, false);
            }
        }

        public static void RemoveLanguageFromPortals(int languageID)
        {
            var controller = new PortalController();
            foreach (PortalInfo portal in controller.GetPortals())
            {
                RemoveLanguageFromPortal(portal.PortalID, languageID);
            }
        }

        public static void RemoveLanguagesFromPortal(int portalID)
        {
            foreach (Locale locale in LocaleController.Instance.GetLocales(portalID).Values)
            {
                RemoveLanguageFromPortal(portalID, locale.LanguageId);
            }
        }

        public static void SaveLanguage(Locale locale)
        {
            SaveLanguage(locale, true);
        }


        public static void SaveLanguage(Locale locale, bool clearCache)
        {
            var objEventLog = new EventLogController();
            if (locale.LanguageId == Null.NullInteger)
            {
                locale.LanguageId = DataProvider.Instance().AddLanguage(locale.Code, locale.Text, locale.Fallback, UserController.GetCurrentUserInfo().UserID);
                objEventLog.AddLog(locale, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.LANGUAGE_CREATED);
            }
            else
            {
                DataProvider.Instance().UpdateLanguage(locale.LanguageId, locale.Code, locale.Text, locale.Fallback, UserController.GetCurrentUserInfo().UserID);
                objEventLog.AddLog(locale, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.LANGUAGE_UPDATED);
            }
            if (clearCache)
                DataCache.ClearHostCache(true);
        }

        public static void SetLanguage(string value)
        {
            try
            {
                HttpResponse response = HttpContext.Current.Response;
                if (response == null)
                {
                    return;
                }

                //save the page culture as a cookie
                HttpCookie cookie = response.Cookies.Get("language");
                if ((cookie == null))
                {
                    if (!String.IsNullOrEmpty(value))
                    {
                        cookie = new HttpCookie("language", value);
                        response.Cookies.Add(cookie);
                    }
                }
                else
                {
                    cookie.Value = value;
                    if (!String.IsNullOrEmpty(value))
                    {
                        response.Cookies.Set(cookie);
                    }
                    else
                    {
                        response.Cookies.Remove("language");
                    }
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        ///   Sets the culture codes on the current Thread
        /// </summary>
        /// <param name = "cultureInfo">Culture Info for the current page</param>
        /// <param name = "portalSettings">The current portalSettings</param>
        /// <remarks>
        ///   This method will configure the Thread culture codes.  Any page which does not derive from PageBase should
        ///   be sure to call this method in OnInit to ensure localiztion works correctly.  See the TelerikDialogHandler for an example.
        /// </remarks>
        public static void SetThreadCultures(CultureInfo cultureInfo, PortalSettings portalSettings)
        {
            if (cultureInfo == null)
            {
                throw new ArgumentNullException("cultureInfo");
            }

			Thread.CurrentThread.CurrentCulture = cultureInfo;

			if (portalSettings != null && portalSettings.ContentLocalizationEnabled &&
						HttpContext.Current.Request.IsAuthenticated &&
						portalSettings.AllowUserUICulture)
			{
				Thread.CurrentThread.CurrentUICulture = GetUserUICulture(cultureInfo, portalSettings);
			}
			else
			{
				Thread.CurrentThread.CurrentUICulture = cultureInfo;
			}
		}

		/// <summary>
		/// When portal allows users to select their preferred UI language, this method
		/// will return the user ui preferred language if defined. Otherwise defaults
		/// to the current culture
		/// </summary>
		/// <param name="currentCulture">Current culture</param>
		/// <param name="portalSettings">PortalSettings for the current request</param>
		/// <returns></returns>
		private static CultureInfo GetUserUICulture(CultureInfo currentCulture, PortalSettings portalSettings)
		{
			CultureInfo uiCulture = currentCulture;
			try
			{
				object oCulture = Personalization.Personalization.GetProfile("Usability", "UICulture");
				if (oCulture != null)
				{
					CultureInfo ci = GetCultureFromString(portalSettings.PortalId, oCulture.ToString());
					if (ci != null)
						uiCulture = ci;
				}
			}
			catch
			{
				// UserCulture seems not valid anymore, update to current culture
				Personalization.Personalization.SetProfile("Usability", "UICulture", currentCulture.Name);
			}
			return uiCulture;
		}

        /// <summary>
        /// Maps the culture code string into the corresponding language ID in the
        /// database. In case there is no language defined in the systen with the
        /// passed code, -1 (<see cref="Null.NullInteger"/>) is returned.
        /// </summary>
        /// <param name="cultureCode">The culture to get the language ID for.</param>
        /// <returns>Language ID integer</returns>
        public static int GetCultureLanguageID(string cultureCode)
        {
            var locale = LocaleController.Instance.GetLocale(cultureCode);
            return locale != null ? locale.LanguageId : Null.NullInteger;
        }
        #endregion

        #region Obsolete

        [Obsolete("Deprecated in DNN 5.0. Replaced by GetLocales().")]
        public static LocaleCollection GetEnabledLocales()
        {
            PortalSettings objPortalSettings = PortalController.GetCurrentPortalSettings();
            var enabledLocales = new LocaleCollection();
            foreach (KeyValuePair<string, Locale> kvp in LocaleController.Instance.GetLocales(objPortalSettings.PortalId))
            {
                enabledLocales.Add(kvp.Key, kvp.Value);
            }
            return enabledLocales;
        }

        [Obsolete("Deprecated in DNN 5.0. Replaced by GetLocales().")]
        public static LocaleCollection GetSupportedLocales()
        {
            var supportedLocales = new LocaleCollection();
            foreach (KeyValuePair<string, Locale> kvp in LocaleController.Instance.GetLocales(Null.NullInteger))
            {
                supportedLocales.Add(kvp.Key, kvp.Value);
            }
            return supportedLocales;
        }

        [Obsolete("Deprecated in DNN 5.5. Replaced by LocaleController.GetLocale(code).")]
        public static Locale GetLocale(string code)
        {
            return GetLocale(Null.NullInteger, code);
        }

        [Obsolete("Deprecated in DNN 5.5. Replaced by LocaleController.GetLocale(portalID, code).")]
        public static Locale GetLocale(int portalID, string code)
        {
            Dictionary<string, Locale> dicLocales = LocaleController.Instance.GetLocales(portalID);
            Locale language = null;

            if (dicLocales != null)
            {
                dicLocales.TryGetValue(code, out language);
            }

            return language;
        }

        [Obsolete("Deprecated in DNN 5.5. Replaced by LocaleController.GetLocale(languageID).")]
        public static Locale GetLocaleByID(int languageID)
        {
            Dictionary<string, Locale> dicLocales = LocaleController.Instance.GetLocales(Null.NullInteger);
            Locale language = null;

            foreach (KeyValuePair<string, Locale> kvp in dicLocales)
            {
                if (kvp.Value.LanguageId == languageID)
                {
                    language = kvp.Value;
                    break;
                }
            }

            return language;
        }

        [Obsolete("Deprecated in DNN 5.5. Replaced by LocaleController.GetLocales(portalID).")]
        public static Dictionary<string, Locale> GetLocales(int portalID)
        {
            return LocaleController.Instance.GetLocales(portalID);
        }

        [Obsolete("Deprecated in DNN 6.0. Replaced by SystemTimeZone and use of .NET TimeZoneInfo class")]
        public static NameValueCollection GetTimeZones(string language)
        {
            language = language.ToLower();
            string cacheKey = "dotnetnuke-" + language + "-timezones";
            string translationFile;
            if (language == SystemLocale.ToLower())
            {
                translationFile = TimezonesFile;
            }
            else
            {
                translationFile = TimezonesFile.Replace(".xml", "." + language + ".xml");
            }
            var timeZones = (NameValueCollection)DataCache.GetCache(cacheKey);
            if (timeZones == null)
            {
                string filePath = HttpContext.Current.Server.MapPath(translationFile);
                timeZones = new NameValueCollection();
                if (File.Exists(filePath) == false)
                {
                    return timeZones;
                }
                var dp = new DNNCacheDependency(filePath);
                try
                {
                    var d = new XmlDocument();
                    d.Load(filePath);
                    foreach (XmlNode n in d.SelectSingleNode("root").ChildNodes)
                    {
                        if (n.NodeType != XmlNodeType.Comment)
                        {
                            timeZones.Add(n.Attributes["name"].Value, n.Attributes["key"].Value);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                }
                if (Host.PerformanceSetting != Globals.PerformanceSettings.NoCaching)
                {
                    DataCache.SetCache(cacheKey, timeZones, dp);
                }
            }
            return timeZones;
        }

        [Obsolete("Deprecated in DNN 5.5.  Replcaed by LocaleController.IsEnabled()")]
        public static bool LocaleIsEnabled(Locale locale)
        {
            return LocaleIsEnabled(locale.Code);
        }

        [Obsolete("Deprecated in DNN 5.5.  Replcaed by LocaleController.IsEnabled()")]
        public static bool LocaleIsEnabled(string localeCode)
        {
            PortalSettings _Settings = PortalController.GetCurrentPortalSettings();

            return LocaleController.Instance.IsEnabled(ref localeCode, _Settings.PortalId);
        }

        [Obsolete("Deprecated in DNN 5.0. Replaced by LocalizeControlTitle(IModuleControl).")]
        public static string LocalizeControlTitle(string controlTitle, string controlSrc, string Key)
        {
            string reskey;
            reskey = "ControlTitle_" + Key.ToLower() + ".Text";
            string resFile = controlSrc.Substring(0, controlSrc.LastIndexOf("/") + 1) + LocalResourceDirectory +
                             controlSrc.Substring(controlSrc.LastIndexOf("/"), controlSrc.LastIndexOf(".") - controlSrc.LastIndexOf("/"));
            if (resFile.StartsWith("DesktopModules"))
            {
                resFile = "~/" + resFile;
            }
            string localizedvalue = GetString(reskey, resFile);
            if (localizedvalue != null)
            {
                return localizedvalue;
            }
            else
            {
                return controlTitle;
            }
        }

        [Obsolete("Deprecated in DNN 5.0. This does nothing now as the Admin Tabs are treated like any other tab.")]
        public static void LocalizePortalSettings()
        {
        }

        [Obsolete("Deprecated in DNN 6.0. Replaced by SystemTimeZone and use of .NET TimeZoneInfo class")]
        public static int SystemTimeZoneOffset
        {
            get
            {
                return -480;
            }
        }

        [Obsolete("Deprecated in DNN 6.0. Replaced by SystemTimeZone and use of .NET TimeZoneInfo class")]
        public static string TimezonesFile
        {
            get
            {
                return ApplicationResourceDirectory + "/TimeZones.xml";
            }
        }

        [Obsolete("Deprecated in DNN 5.0. Replaced by Host.EnableBrowserLanguage OR PortalSettings.EnableBrowserLanguage")]
        public static bool UseBrowserLanguage()
        {
            return PortalController.GetCurrentPortalSettings().EnableBrowserLanguage;
        }

        [Obsolete("Deprecated in DNN 5.0. Replaced by Host.EnableUrlLanguage OR PortalSettings.EnableUrlLanguage")]
        public static bool UseLanguageInUrl()
        {
            return PortalController.GetCurrentPortalSettings().EnableUrlLanguage;
        }

        [Obsolete("Deprecated in DNN 6.0. Replaced by new DnnTimeZoneComboBox control and use of .NET TimeZoneInfo class")]
        public static void LoadTimeZoneDropDownList(DropDownList list, string language, string selectedValue)
        {
            NameValueCollection timeZones = GetTimeZones(language);
            //If no Timezones defined get the System Locale Time Zones
            if (timeZones.Count == 0)
            {
                timeZones = GetTimeZones(SystemLocale.ToLower());
            }
            int i;
            for (i = 0; i <= timeZones.Keys.Count - 1; i++)
            {
                list.Items.Add(new ListItem(timeZones.GetKey(i), timeZones.Get(i)));
            }

            //select the default item
            if (selectedValue != null)
            {
                ListItem item = list.Items.FindByValue(selectedValue);
                if (item == null)
                {
                    //Try system default
                    item = list.Items.FindByValue(SystemTimeZoneOffset.ToString());
                }
                if (item != null)
                {
                    list.SelectedIndex = -1;
                    item.Selected = true;
                }
            }
        }

        #endregion
    }
}