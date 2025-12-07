// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System.Collections;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    //using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ResourceManager;

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        private const string SearchFileName = "Search.ascx";

        public static MvcHtmlString Search(
            this HtmlHelper<PageModel> helper,
            string id,
            bool useDropDownList = false,
            bool showWeb = true,
            bool showSite = true,
            string cssClass = "",
            string submit = null,
            string webIconURL = null,
            string webText = null,
            string webToolTip = null,
            string webUrl = null,
            string siteText = null,
            bool useWebForSite = false,
            bool enableWildSearch = true,
            int minCharRequired = 2,
            int autoSearchDelayInMilliSecond = 400)
        {
            var navigationManager = helper.ViewData.Model.NavigationManager;
            //TODO: CSP - enable when CSP implementation is ready
            var nonce = string.Empty; // helper.ViewData.Model.ContentSecurityPolicy.Nonce;
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            var controller = HtmlHelpers.GetClientResourcesController(helper);
            controller.RegisterStylesheet("~/Resources/Search/SearchSkinObjectPreview.css", FileOrder.Css.ModuleCss);
            controller.CreateScript("~/Resources/Search/SearchSkinObjectPreview.js")
                .SetDefer()
                .Register();

            var searchType = "S";
            /*
            if (this.WebRadioButton.Visible)
            {
                if (this.WebRadioButton.Checked)
                {
                    this.SearchType = "W";
                }
            }
            */
            if (string.IsNullOrEmpty(webIconURL))
            {
                webIconURL = IconController.IconURL("GoogleSearch");
            }

            if (string.IsNullOrEmpty(webText))
            {
                webUrl = Localization.GetString("Web", SkinHelpers.GetSkinsResourceFile(SearchFileName));
            }

            if (string.IsNullOrEmpty(webToolTip))
            {
                webUrl = Localization.GetString("Web.ToolTip", SkinHelpers.GetSkinsResourceFile(SearchFileName));
            }

            if (string.IsNullOrEmpty(webUrl))
            {
                webUrl = Localization.GetString("URL", SkinHelpers.GetSkinsResourceFile(SearchFileName));
            }

            var searchUrl = SkinHelpers.ExecuteSearchUrl(string.Empty, searchType, useWebForSite, webUrl, navigationManager);
            if (!useDropDownList)
            {
                return BuildClassicSearch(id, showWeb, showSite, cssClass, submit, webText, siteText, enableWildSearch, minCharRequired, autoSearchDelayInMilliSecond, searchUrl, nonce);
            }

            return BuildDropDownSearch(id, cssClass, submit, webText, siteText, enableWildSearch, minCharRequired, autoSearchDelayInMilliSecond, searchUrl, nonce);
        }

        private static MvcHtmlString BuildClassicSearch(
            string id,
            bool showWeb,
            bool showSite,
            string cssClass,
            string submit,
            string webText,
            string siteText,
            bool enableWildSearch,
            int minCharRequired,
            int autoSearchDelayInMilliSecond,
            string searchUrl,
            string nonce)
        {
            var container = new TagBuilder("span");
            container.GenerateId("dnn_" + id + "_ClassicSearch");
            var containerId = container.Attributes["id"];

            if (showWeb)
            {
                var radio = new TagBuilder("input");
                radio.Attributes["type"] = "radio";
                radio.Attributes["name"] = "SearchType";
                radio.Attributes["value"] = "W";
                radio.Attributes["id"] = "dnn_" + id + "WebRadioButton";
                radio.Attributes["class"] = cssClass;
                radio.Attributes["checked"] = "checked";
                container.InnerHtml += radio.ToString(TagRenderMode.SelfClosing);

                var label = new TagBuilder("label");
                label.Attributes["for"] = "WebRadioButton";
                label.SetInnerText(webText ?? Localization.GetString("Web", GetSkinsResourceFile(SearchFileName)));
                container.InnerHtml += label.ToString();
            }

            if (showSite)
            {
                var radio = new TagBuilder("input");
                radio.Attributes["type"] = "radio";
                radio.Attributes["name"] = "SearchType";
                radio.Attributes["value"] = "S";
                radio.Attributes["id"] = "dnn_" + id + "SiteRadioButton";
                radio.Attributes["class"] = cssClass;
                container.InnerHtml += radio.ToString(TagRenderMode.SelfClosing);

                var label = new TagBuilder("label");
                label.Attributes["for"] = "SiteRadioButton";
                label.SetInnerText(siteText ?? Localization.GetString("Site", GetSkinsResourceFile(SearchFileName)));
                container.InnerHtml += label.ToString();
            }

            container.InnerHtml += BuildSearchInput(id, "txtSearch", "NormalTextBox");
            container.InnerHtml += BuildSearchButton(cssClass, submit, searchUrl);

            return new MvcHtmlString(container.ToString() + GetInitScript(false, enableWildSearch, minCharRequired, autoSearchDelayInMilliSecond, containerId, nonce));
        }

        private static MvcHtmlString BuildDropDownSearch(
            string id,
            string cssClass,
            string submit,
            string webText,
            string siteText,
            bool enableWildSearch,
            int minCharRequired,
            int autoSearchDelayInMilliSecond,
            string searchUrl,
            string nonce)
        {
            var container = new TagBuilder("div");
            container.GenerateId("dnn" + id + "DropDownSearch");
            container.AddCssClass("SearchContainer");
            var containerId = container.Attributes["id"];

            var searchBorder = new TagBuilder("div");
            searchBorder.AddCssClass("SearchBorder");

            var searchIcon = new TagBuilder("div");
            searchIcon.GenerateId("dnn" + id + "SearchIcon");
            searchIcon.AddCssClass("SearchIcon");

            var img = new TagBuilder("img");
            img.Attributes["src"] = IconController.IconURL("Action");
            img.Attributes["alt"] = Localization.GetString("DropDownGlyph.AltText", GetSkinsResourceFile(SearchFileName));
            searchIcon.InnerHtml = img.ToString(TagRenderMode.SelfClosing);

            searchBorder.InnerHtml += searchIcon.ToString();
            searchBorder.InnerHtml += BuildSearchInput(id, "txtSearchNew", "SearchTextBox");

            var choices = new TagBuilder("ul");
            choices.GenerateId("SearchChoices");

            var siteLi = new TagBuilder("li");
            siteLi.GenerateId("dnn" + id + "_SearchIconSite");
            siteLi.SetInnerText(siteText ?? Localization.GetString("Site", GetSkinsResourceFile(SearchFileName)));
            choices.InnerHtml += siteLi.ToString();

            var webLi = new TagBuilder("li");
            webLi.GenerateId("SearchIconWeb");
            webLi.SetInnerText(webText ?? Localization.GetString("Web", GetSkinsResourceFile(SearchFileName)));
            choices.InnerHtml += webLi.ToString();

            searchBorder.InnerHtml += choices.ToString();
            container.InnerHtml = searchBorder.ToString() + BuildSearchButton(cssClass, submit, searchUrl);

            return new MvcHtmlString(container.ToString() + GetInitScript(true, enableWildSearch, minCharRequired, autoSearchDelayInMilliSecond, containerId, nonce));
        }

        private static string BuildSearchInput(string id, string inputId, string cssClass)
        {
            var container = new TagBuilder("span");
            container.AddCssClass("searchInputContainer");
            container.Attributes["data-moreresults"] = GetSeeMoreText();
            container.Attributes["data-noresult"] = GetNoResultText();

            var input = new TagBuilder("input");
            input.Attributes["type"] = "text";
            input.Attributes["id"] = "dnn_" + id + "_" + inputId;
            input.Attributes["class"] = cssClass;
            input.Attributes["maxlength"] = "255";
            input.Attributes["autocomplete"] = "off";
            input.Attributes["placeholder"] = GetPlaceholderText();
            input.Attributes["aria-label"] = "Search";

            var clear = new TagBuilder("a");
            clear.AddCssClass("dnnSearchBoxClearText");
            clear.Attributes["title"] = GetClearQueryText();
            clear.Attributes["href"] = "#";

            container.InnerHtml = input.ToString(TagRenderMode.SelfClosing) + clear.ToString();
            return container.ToString();
        }

        private static string BuildSearchButton(string cssClass, string submit, string searchUrl)
        {
            var button = new TagBuilder("a");
            if (string.IsNullOrEmpty(cssClass))
            {
                button.AddCssClass("SkinObject");
            }
            else
            {
                button.AddCssClass(cssClass);
            }
                
            button.Attributes["href"] = searchUrl; // "#";
            button.InnerHtml = submit ?? Localization.GetString("Search", GetSkinsResourceFile(SearchFileName));
            return button.ToString();
        }

        private static string GetInitScript(bool useDropDownList, bool enableWildSearch, int minCharRequired, int autoSearchDelayInMilliSecond, string id, string nonce)
        {
            var nonceAttribute = string.Empty;
            if (!string.IsNullOrEmpty(nonce))
            {
                nonceAttribute = $"nonce=\"{nonce}\"";
            }
            return string.Format(
                @"
                <script {8} >
                    $(function() {{
                        if (typeof dnn != 'undefined' && typeof dnn.searchSkinObject != 'undefined') {{
                            var searchSkinObject = new dnn.searchSkinObject({{
                                delayTriggerAutoSearch: {0},
                                minCharRequiredTriggerAutoSearch: {1},
                                searchType: '{2}',
                                enableWildSearch: {3},
                                cultureCode: '{4}',
                                portalId: {5}
                            }});
                            searchSkinObject.init();
                            {6}
                            $('#{7} .SearchButton').click(function(){{ 
                                window.location = $(this).attr('href')+'?Search='+$('#{7} input').val(); 
                                return false; 
                            }})
                        }}
                    }});
                </script>",
                autoSearchDelayInMilliSecond,
                minCharRequired,
                "S",
                enableWildSearch.ToString().ToLowerInvariant(),
                System.Threading.Thread.CurrentThread.CurrentCulture.ToString(),
                PortalSettings.Current.PortalId,
                useDropDownList ? "if (typeof dnn.initDropdownSearch != 'undefined') { dnn.initDropdownSearch(searchSkinObject); }" : string.Empty,
                id,
                nonceAttribute);
        }

        private static string GetSeeMoreText()
        {
            return Localization.GetSafeJSString("SeeMoreResults", GetSkinsResourceFile(SearchFileName));
        }

        private static string GetNoResultText()
        {
            return Localization.GetSafeJSString("NoResult", GetSkinsResourceFile(SearchFileName));
        }

        private static string GetClearQueryText()
        {
            return Localization.GetSafeJSString("SearchClearQuery", GetSkinsResourceFile(SearchFileName));
        }

        private static string GetPlaceholderText()
        {
            return Localization.GetSafeJSString("Placeholder", GetSkinsResourceFile(SearchFileName));
        }

        private static string ExecuteSearchUrl(string searchText, string searchType, bool useWebForSite, string webURL, INavigationManager navigationManager)
        {
            PortalSettings portalSettings = PortalSettings.Current;

            int searchTabId = SkinHelpers.GetSearchTabId(portalSettings);

            if (searchTabId == Null.NullInteger)
            {
                return string.Empty;
            }

            string strURL;
            if (!string.IsNullOrEmpty(searchText))
            {
                switch (searchType)
                {
                    case "S":
                        // site
                        if (useWebForSite)
                        {
                            /*
                            strURL = this.SiteURL;
                            if (!string.IsNullOrEmpty(strURL))
                            {
                                strURL = strURL.Replace("[TEXT]", this.Server.UrlEncode(searchText));
                                strURL = strURL.Replace("[DOMAIN]", this.Request.Url.Host);
                                UrlUtils.OpenNewWindow(this.Page, this.GetType(), strURL);
                            }
                            */
                            return string.Empty;
                        }
                        else
                        {
                            if (Host.UseFriendlyUrls)
                            {
                                return navigationManager.NavigateURL(searchTabId) /* + "?Search=" + this.Server.UrlEncode(searchText)*/;
                            }
                            else
                            {
                                return navigationManager.NavigateURL(searchTabId) /* + "&Search=" + this.Server.UrlEncode(searchText)*/;
                            }
                        }

                    case "W":
                        // web
                        strURL = webURL;
                        if (!string.IsNullOrEmpty(strURL))
                        {
                            /*
                            strURL = strURL.Replace("[TEXT]", this.Server.UrlEncode(searchText));
                            strURL = strURL.Replace("[DOMAIN]", string.Empty);
                            UrlUtils.OpenNewWindow(this.Page, this.GetType(), strURL);
                            */
                        }

                        return string.Empty;
                }
            }
            else
            {
                if (Host.UseFriendlyUrls)
                {
                    return navigationManager.NavigateURL(searchTabId);
                }
                else
                {
                    return navigationManager.NavigateURL(searchTabId);
                }
            }

            return string.Empty;
        }

        private static int GetSearchTabId(PortalSettings portalSettings)
        {
            int searchTabId = portalSettings.SearchTabId;
            if (searchTabId == Null.NullInteger)
            {
                ArrayList arrModules = ModuleController.Instance.GetModulesByDefinition(portalSettings.PortalId, "Search Results");
                if (arrModules.Count > 1)
                {
                    foreach (ModuleInfo searchModule in arrModules)
                    {
                        if (searchModule.CultureCode == portalSettings.CultureCode)
                        {
                            searchTabId = searchModule.TabID;
                        }
                    }
                }
                else if (arrModules.Count == 1)
                {
                    searchTabId = ((ModuleInfo)arrModules[0]).TabID;
                }
            }

            return searchTabId;
        }
    }
}
