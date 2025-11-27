// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Language(
            this HtmlHelper<PageModel> helper,
            string cssClass = "",
            string itemTemplate = "",
            string selectedItemTemplate = "",
            string headerTemplate = "",
            string footerTemplate = "",
            string alternateTemplate = "",
            string separatorTemplate = "",
            string commonHeaderTemplate = "",
            string commonFooterTemplate = "",
            bool showMenu = true,
            bool showLinks = false,
            bool useCurrentCultureForTemplate = false)
        {
            var portalSettings = PortalSettings.Current;
            var currentCulture = CultureInfo.CurrentCulture.ToString();
            var templateCulture = useCurrentCultureForTemplate ? currentCulture : "en-US";
            var localResourceFile = GetSkinsResourceFile("Language.ascx");
            var localTokenReplace = new LanguageTokenReplace { resourceFile = localResourceFile };

            var locales = new Dictionary<string, Locale>();
            IEnumerable<ListItem> cultureListItems = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, currentCulture, string.Empty, false);
            foreach (Locale loc in LocaleController.Instance.GetLocales(portalSettings.PortalId).Values)
            {
                string defaultRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", loc.Code), portalSettings.PortalId, "Administrators");
                if (!portalSettings.ContentLocalizationEnabled ||
                    (LocaleIsAvailable(loc, portalSettings) &&
                        (PortalSecurity.IsInRoles(portalSettings.AdministratorRoleName) || loc.IsPublished || PortalSecurity.IsInRoles(defaultRoles))))
                {
                    locales.Add(loc.Code, loc);
                }
            }

            int cultureCount = 0;

            var selectCulture = new TagBuilder("select");
            selectCulture.Attributes.Add("id", "selectCulture");
            selectCulture.Attributes.Add("name", "selectCulture");
            selectCulture.AddCssClass("NormalTextBox");
            selectCulture.Attributes.Add("onchange", "var url = this.options[this.selectedIndex].getAttribute('data-link'); if(url) window.location.href = url;");

            if (!string.IsNullOrEmpty(cssClass))
            {
                selectCulture.AddCssClass(cssClass);
            }

            // TimoBreumelhof: This is not ideal but it works for now.. should really be a UL IMO
            foreach (var cultureItem in cultureListItems)
            {
                cultureCount++;
                if (locales.ContainsKey(cultureItem.Value))
                {
                    var option = new TagBuilder("option");
                    option.Attributes.Add("value", cultureItem.Value);
                    option.Attributes.Add("data-link", ParseTemplate("[URL]", cultureItem.Value, localTokenReplace, currentCulture));
                    option.SetInnerText(cultureItem.Text);
                    if (cultureItem.Value == currentCulture)
                    {
                        option.Attributes.Add("selected", "selected");
                    }

                    selectCulture.InnerHtml += option.ToString();
                }
            }

            if (string.IsNullOrEmpty(commonHeaderTemplate))
            {
                commonHeaderTemplate = Localization.GetString("CommonHeaderTemplate.Default", localResourceFile, templateCulture);
            }

            if (string.IsNullOrEmpty(commonFooterTemplate))
            {
                commonFooterTemplate = Localization.GetString("CommonFooterTemplate.Default", localResourceFile, templateCulture);
            }

            string languageContainer = string.Empty;
            languageContainer += commonHeaderTemplate;

            if (showMenu)
            {
                languageContainer += selectCulture.ToString();
            }

            if (showLinks)
            {
                if (string.IsNullOrEmpty(itemTemplate))
                {
                    itemTemplate = Localization.GetString("ItemTemplate.Default", localResourceFile, templateCulture);
                }

                if (string.IsNullOrEmpty(alternateTemplate))
                {
                    alternateTemplate = Localization.GetString("AlternateTemplate.Default", localResourceFile, templateCulture);
                }

                if (string.IsNullOrEmpty(selectedItemTemplate))
                {
                    selectedItemTemplate = Localization.GetString("SelectedItemTemplate.Default", localResourceFile, templateCulture);
                }

                if (string.IsNullOrEmpty(headerTemplate))
                {
                    headerTemplate = Localization.GetString("HeaderTemplate.Default", localResourceFile, templateCulture);
                }

                if (string.IsNullOrEmpty(footerTemplate))
                {
                    footerTemplate = Localization.GetString("FooterTemplate.Default", localResourceFile, templateCulture);
                }

                if (string.IsNullOrEmpty(separatorTemplate))
                {
                    separatorTemplate = Localization.GetString("SeparatorTemplate.Default", localResourceFile, templateCulture);
                }

                languageContainer += headerTemplate;

                string listItems = string.Empty;

                bool alt = false;
                int i = 0;
                foreach (var locale in locales.Values)
                {
                    if (i > 0 && !string.IsNullOrEmpty(separatorTemplate))
                    {
                        listItems += ParseTemplate(separatorTemplate, "", localTokenReplace, currentCulture);
                    }
                    i++;

                    string listItem = string.Empty;
                    if (locale.Code == currentCulture && !string.IsNullOrEmpty(selectedItemTemplate))
                    {
                        listItem += ParseTemplate(selectedItemTemplate, locale.Code, localTokenReplace, currentCulture);
                    }
                    else
                    {
                        if (alt)
                        {
                            listItem += ParseTemplate(alternateTemplate, locale.Code, localTokenReplace, currentCulture);
                        }
                        else
                        {
                            listItem += ParseTemplate(itemTemplate, locale.Code, localTokenReplace, currentCulture);
                        }

                        alt = !alt;
                    }

                    listItems += listItem;
                }

                languageContainer += listItems;

                languageContainer += footerTemplate;
            }

            languageContainer += commonFooterTemplate;

            if (cultureCount <= 1)
            {
                languageContainer = string.Empty;
            }

            return new MvcHtmlString(languageContainer);
        }

        private static string ParseTemplate(string template, string locale, LanguageTokenReplace localTokenReplace, string currentCulture)
        {
            string strReturnValue = template;
            try
            {
                if (!string.IsNullOrEmpty(locale))
                {
                    localTokenReplace.Language = locale;
                }
                else
                {
                    localTokenReplace.Language = currentCulture;
                }

                strReturnValue = localTokenReplace.ReplaceEnvironmentTokens(strReturnValue);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, HttpContext.Current.Request.RawUrl);
            }

            return strReturnValue;
        }

        private static bool LocaleIsAvailable(Locale locale, PortalSettings portalSettings)
        {
            var tab = portalSettings.ActiveTab;
            if (tab.DefaultLanguageTab != null)
            {
                tab = tab.DefaultLanguageTab;
            }

            var localizedTab = TabController.Instance.GetTabByCulture(tab.TabID, tab.PortalID, locale);

            return localizedTab != null && !localizedTab.IsDeleted && TabPermissionController.CanViewPage(localizedTab);
        }
    }
}
