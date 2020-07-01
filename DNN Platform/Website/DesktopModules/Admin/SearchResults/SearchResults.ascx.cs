// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.SearchResults
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public partial class SearchResults : PortalModuleBase
    {
        private const int DefaultPageIndex = 1;
        private const int DefaultPageSize = 15;
        private const int DefaultSortOption = 0;

        private const string MyFileName = "SearchResults.ascx";

        private IList<string> _searchContentSources;
        private IList<int> _searchPortalIds;

        protected string SearchTerm
        {
            get { return this.Request.QueryString["Search"] ?? string.Empty; }
        }

        protected string TagsQuery
        {
            get { return this.Request.QueryString["Tag"] ?? string.Empty; }
        }

        protected string SearchScopeParam
        {
            get { return this.Request.QueryString["Scope"] ?? string.Empty; }
        }

        protected string[] SearchScope
        {
            get
            {
                var searchScopeParam = this.SearchScopeParam;
                return string.IsNullOrEmpty(searchScopeParam) ? new string[0] : searchScopeParam.Split(',');
            }
        }

        protected string LastModifiedParam
        {
            get { return this.Request.QueryString["LastModified"] ?? string.Empty; }
        }

        protected int PageIndex
        {
            get
            {
                if (string.IsNullOrEmpty(this.Request.QueryString["Page"]))
                {
                    return DefaultPageIndex;
                }

                int pageIndex;
                if (int.TryParse(this.Request.QueryString["Page"], out pageIndex))
                {
                    return pageIndex;
                }

                return DefaultPageIndex;
            }
        }

        protected int PageSize
        {
            get
            {
                if (string.IsNullOrEmpty(this.Request.QueryString["Size"]))
                {
                    return DefaultPageSize;
                }

                int pageSize;
                if (int.TryParse(this.Request.QueryString["Size"], out pageSize))
                {
                    return pageSize;
                }

                return DefaultPageSize;
            }
        }

        protected int SortOption
        {
            get
            {
                if (string.IsNullOrEmpty(this.Request.QueryString["Sort"]))
                {
                    return DefaultSortOption;
                }

                int sortOption;
                if (int.TryParse(this.Request.QueryString["Sort"], out sortOption))
                {
                    return sortOption;
                }

                return DefaultSortOption;
            }
        }

        protected string CheckedExactSearch
        {
            get
            {
                var paramExactSearch = this.Request.QueryString["ExactSearch"];

                if (!string.IsNullOrEmpty(paramExactSearch) && paramExactSearch.ToLowerInvariant() == "y")
                {
                    return "checked=\"true\"";
                }

                return string.Empty;
            }
        }

        protected string LinkTarget
        {
            get
            {
                string settings = Convert.ToString(this.Settings["LinkTarget"]);
                return string.IsNullOrEmpty(settings) || settings == "0" ? string.Empty : " target=\"_blank\" ";
            }
        }

        protected string ShowDescription => this.GetBooleanSetting("ShowDescription", true).ToString().ToLowerInvariant();

        protected string ShowSnippet => this.GetBooleanSetting("ShowSnippet", true).ToString().ToLowerInvariant();

        protected string ShowSource => this.GetBooleanSetting("ShowSource", true).ToString().ToLowerInvariant();

        protected string ShowLastUpdated => this.GetBooleanSetting("ShowLastUpdated", true).ToString().ToLowerInvariant();

        protected string ShowTags => this.GetBooleanSetting("ShowTags", true).ToString().ToLowerInvariant();

        protected string MaxDescriptionLength => this.GetIntegerSetting("MaxDescriptionLength", 100).ToString();

        protected IList<string> SearchContentSources
        {
            get
            {
                if (this._searchContentSources == null)
                {
                    IList<int> portalIds = this.SearchPortalIds;
                    var list = new List<SearchContentSource>();
                    foreach (int portalId in portalIds)
                    {
                        IEnumerable<SearchContentSource> crawlerList =
                            InternalSearchController.Instance.GetSearchContentSourceList(portalId);
                        foreach (SearchContentSource src in crawlerList)
                        {
                            if (src.IsPrivate)
                            {
                                continue;
                            }

                            if (list.All(r => r.LocalizedName != src.LocalizedName))
                            {
                                list.Add(src);
                            }
                        }
                    }

                    List<string> configuredList = null;

                    if (!string.IsNullOrEmpty(Convert.ToString(this.Settings["ScopeForFilters"])))
                    {
                        configuredList = Convert.ToString(this.Settings["ScopeForFilters"]).Split('|').ToList();
                    }

                    this._searchContentSources = new List<string>();

                    // add other searchable module defs
                    foreach (SearchContentSource contentSource in list)
                    {
                        if (configuredList == null ||
                            configuredList.Any(l => l.Equals(contentSource.LocalizedName)))
                        {
                            if (!this._searchContentSources.Equals(contentSource.LocalizedName))
                            {
                                this._searchContentSources.Add(contentSource.LocalizedName);
                            }
                        }
                    }
                }

                return this._searchContentSources;
            }
        }

        protected string DefaultText
        {
            get { return Localization.GetSafeJSString("DefaultText", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string NoResultsText
        {
            get { return Localization.GetSafeJSString("NoResults", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string AdvancedText
        {
            get { return Localization.GetSafeJSString("Advanced", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string SourceText
        {
            get { return Localization.GetSafeJSString("Source", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string TagsText
        {
            get { return Localization.GetSafeJSString("Tags", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string AuthorText
        {
            get { return Localization.GetSafeJSString("Author", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string LikesText
        {
            get { return Localization.GetSafeJSString("Likes", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string ViewsText
        {
            get { return Localization.GetSafeJSString("Views", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string CommentsText
        {
            get { return Localization.GetSafeJSString("Comments", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string RelevanceText
        {
            get { return Localization.GetSafeJSString("Relevance", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string DateText
        {
            get { return Localization.GetSafeJSString("Date", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string SearchButtonText
        {
            get { return Localization.GetSafeJSString("btnSearch", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string ClearButtonText
        {
            get { return Localization.GetSafeJSString("btnClear", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string AdvancedSearchHintText
        {
            get { return Localization.GetString("AdvancedSearchHint", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string LinkAdvancedTipText
        {
            get { return Localization.GetString("linkAdvancedTip", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string LastModifiedText
        {
            get { return Localization.GetString("LastModified", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string ResultsPerPageText
        {
            get { return Localization.GetString("ResultPerPage", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string AddTagText
        {
            get { return Localization.GetSafeJSString("AddTag", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string ResultsCountText
        {
            get { return Localization.GetSafeJSString("ResultsCount", Localization.GetResourceFile(this, MyFileName)); }
        }

        protected string CurrentPageIndexText
        {
            get
            {
                return Localization.GetSafeJSString("CurrentPageIndex", Localization.GetResourceFile(this, MyFileName));
            }
        }

        protected string CultureCode { get; set; }

        private IList<int> SearchPortalIds
        {
            get
            {
                if (this._searchPortalIds == null)
                {
                    this._searchPortalIds = new List<int>();
                    if (!string.IsNullOrEmpty(Convert.ToString(this.Settings["ScopeForPortals"])))
                    {
                        List<string> list = Convert.ToString(this.Settings["ScopeForPortals"]).Split('|').ToList();
                        foreach (string l in list)
                        {
                            this._searchPortalIds.Add(Convert.ToInt32(l));
                        }
                    }
                    else
                    {
                        this._searchPortalIds.Add(this.PortalId); // no setting, just search current portal by default
                    }
                }

                return this._searchPortalIds;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/scripts/dnn.searchBox.js");
            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/stylesheets/dnn.searchBox.css", FileOrder.Css.ModuleCss);
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/admin/SearchResults/dnn.searchResult.js");

            this.CultureCode = Thread.CurrentThread.CurrentCulture.ToString();

            foreach (string o in this.SearchContentSources)
            {
                var item = new ListItem(o, o) { Selected = this.CheckedScopeItem(o) };
                this.SearchScopeList.Items.Add(item);
            }

            this.SearchScopeList.Options.Localization["AllItemsChecked"] = Localization.GetString(
                "AllFeaturesSelected",
                Localization.GetResourceFile(this, MyFileName));

            var pageSizeItem = this.ResultsPerPageList.FindItemByValue(this.PageSize.ToString());
            if (pageSizeItem != null)
            {
                pageSizeItem.Selected = true;
            }

            this.SetLastModifiedFilter();
        }

        private bool CheckedScopeItem(string scopeItemName)
        {
            var searchScope = this.SearchScope;
            return searchScope.Length == 0 || searchScope.Any(x => x == scopeItemName);
        }

        private void SetLastModifiedFilter()
        {
            var lastModifiedParam = this.LastModifiedParam;

            if (!string.IsNullOrEmpty(lastModifiedParam))
            {
                var item = this.AdvnacedDatesList.Items.Cast<ListItem>().FirstOrDefault(x => x.Value == lastModifiedParam);
                if (item != null)
                {
                    item.Selected = true;
                }
            }
        }

        private bool GetBooleanSetting(string settingName, bool defaultValue)
        {
            if (this.Settings.ContainsKey(settingName) && !string.IsNullOrEmpty(Convert.ToString(this.Settings[settingName])))
            {
                return Convert.ToBoolean(this.Settings[settingName]);
            }

            return defaultValue;
        }

        private int GetIntegerSetting(string settingName, int defaultValue)
        {
            var settingValue = Convert.ToString(this.Settings[settingName]);
            if (!string.IsNullOrEmpty(settingValue) && Regex.IsMatch(settingValue, "^\\d+$"))
            {
                return Convert.ToInt32(settingValue);
            }

            return defaultValue;
        }
    }
}
