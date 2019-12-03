#region Copyright

//
// DotNetNuke® - https://www.dnnsoftware.com
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

#region Usings

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

#endregion

namespace DotNetNuke.Modules.SearchResults
{
    public partial class SearchResults : PortalModuleBase
    {
        private const int DefaultPageIndex = 1;
        private const int DefaultPageSize = 15;
        private const int DefaultSortOption = 0;

        private IList<string> _searchContentSources;
        private IList<int> _searchPortalIds;

        protected string SearchTerm
        {
            get { return Request.QueryString["Search"] ?? string.Empty; }
        }

        protected string TagsQuery
        {
            get { return Request.QueryString["Tag"] ?? string.Empty; }
        }

        protected string SearchScopeParam
        {
            get { return Request.QueryString["Scope"] ?? string.Empty; }
        }

        protected string [] SearchScope
        {
            get
            {
                var searchScopeParam = SearchScopeParam;
                return string.IsNullOrEmpty(searchScopeParam) ? new string[0] : searchScopeParam.Split(',');
            }
        }

        protected string LastModifiedParam
        {
            get { return Request.QueryString["LastModified"] ?? string.Empty; }
        }

        protected int PageIndex
        {
            get
            {
                if (string.IsNullOrEmpty(Request.QueryString["Page"]))
                {
                    return DefaultPageIndex;
                }

                int pageIndex;
                if (Int32.TryParse(Request.QueryString["Page"], out pageIndex))
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
                if (string.IsNullOrEmpty(Request.QueryString["Size"]))
                {
                    return DefaultPageSize;
                }

                int pageSize;
                if (Int32.TryParse(Request.QueryString["Size"], out pageSize))
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
                if (string.IsNullOrEmpty(Request.QueryString["Sort"]))
                {
                    return DefaultSortOption;
                }

                int sortOption;
                if (Int32.TryParse(Request.QueryString["Sort"], out sortOption))
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
                var paramExactSearch = Request.QueryString["ExactSearch"];

                if (!string.IsNullOrEmpty(paramExactSearch) && paramExactSearch.ToLowerInvariant() == "y")
                {
                    return "checked=\"true\"";
                }
                return "";
            }
        }

        protected string LinkTarget
        {
            get
            {
                string settings = Convert.ToString(Settings["LinkTarget"]);
                return string.IsNullOrEmpty(settings) || settings == "0" ? string.Empty : " target=\"_blank\" ";
            }
        }

        protected string ShowDescription => GetBooleanSetting("ShowDescription", true).ToString().ToLowerInvariant();

        protected string ShowSnippet => GetBooleanSetting("ShowSnippet", true).ToString().ToLowerInvariant();

        protected string ShowSource => GetBooleanSetting("ShowSource", true).ToString().ToLowerInvariant();

        protected string ShowLastUpdated => GetBooleanSetting("ShowLastUpdated", true).ToString().ToLowerInvariant();

        protected string ShowTags => GetBooleanSetting("ShowTags", true).ToString().ToLowerInvariant();

        protected string MaxDescriptionLength => GetIntegerSetting("MaxDescriptionLength", 100).ToString();

        private IList<int> SearchPortalIds
        {
            get
            {
                if (_searchPortalIds == null)
                {
                    _searchPortalIds = new List<int>();
                    if (!string.IsNullOrEmpty(Convert.ToString(Settings["ScopeForPortals"])))
                    {
                        List<string> list = Convert.ToString(Settings["ScopeForPortals"]).Split('|').ToList();
                        foreach (string l in list) _searchPortalIds.Add(Convert.ToInt32(l));
                    }
                    else
                    {
                        _searchPortalIds.Add(PortalId); // no setting, just search current portal by default
                    }
                }

                return _searchPortalIds;
            }
        }

        protected IList<string> SearchContentSources
        {
            get
            {
                if (_searchContentSources == null)
                {
                    IList<int> portalIds = SearchPortalIds;
                    var list = new List<SearchContentSource>();
                    foreach (int portalId in portalIds)
                    {
                        IEnumerable<SearchContentSource> crawlerList =
                            InternalSearchController.Instance.GetSearchContentSourceList(portalId);
                        foreach (SearchContentSource src in crawlerList)
                        {
                            if (src.IsPrivate) continue;
                            if (list.All(r => r.LocalizedName != src.LocalizedName))
                            {
                                list.Add(src);
                            }
                        }
                    }

                    List<string> configuredList = null;

                    if (!string.IsNullOrEmpty(Convert.ToString(Settings["ScopeForFilters"])))
                    {
                        configuredList = Convert.ToString(Settings["ScopeForFilters"]).Split('|').ToList();
                    }

                    _searchContentSources = new List<string>();

                    // add other searchable module defs
                    foreach (SearchContentSource contentSource in list)
                    {
                        if (configuredList == null ||
                            configuredList.Any(l => l.Contains(contentSource.LocalizedName)))
                        {
                            if (!_searchContentSources.Contains(contentSource.LocalizedName))
                            {
                                _searchContentSources.Add(contentSource.LocalizedName);
                            }
                        }
                    }
                }

                return _searchContentSources;
            }
        }

        #region localized string

        private const string MyFileName = "SearchResults.ascx";

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

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.searchBox.js");
            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/stylesheets/dnn.searchBox.css", FileOrder.Css.ModuleCss);
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/admin/SearchResults/dnn.searchResult.js");

            CultureCode = Thread.CurrentThread.CurrentCulture.ToString();

            foreach (string o in SearchContentSources)
            {
                var item = new ListItem(o, o) {Selected = CheckedScopeItem(o)};
                SearchScopeList.Items.Add(item);
            }

            SearchScopeList.Options.Localization["AllItemsChecked"] = Localization.GetString("AllFeaturesSelected",
                Localization.GetResourceFile(this, MyFileName));

            var pageSizeItem = ResultsPerPageList.FindItemByValue(PageSize.ToString());
            if (pageSizeItem != null)
            {
                pageSizeItem.Selected = true;
            }

            SetLastModifiedFilter();
        }

        private bool CheckedScopeItem(string scopeItemName)
        {
            var searchScope = SearchScope;
            return searchScope.Length == 0 || searchScope.Any(x => x == scopeItemName);
        }

        private void SetLastModifiedFilter()
        {
            var lastModifiedParam = LastModifiedParam;

            if (!string.IsNullOrEmpty(lastModifiedParam))
            {
                var item = AdvnacedDatesList.Items.Cast<ListItem>().FirstOrDefault(x => x.Value == lastModifiedParam);
                if (item != null)
                {
                    item.Selected = true;
                }
            }
        }

        private bool GetBooleanSetting(string settingName, bool defaultValue)
        {
            if (Settings.ContainsKey(settingName) && !string.IsNullOrEmpty(Convert.ToString(Settings[settingName])))
            {
                return Convert.ToBoolean(Settings[settingName]);
            }

            return defaultValue;
        }

        private int GetIntegerSetting(string settingName, int defaultValue)
        {
            var settingValue = Convert.ToString(Settings[settingName]);
            if (!string.IsNullOrEmpty(settingValue) && Regex.IsMatch(settingValue, "^\\d+$"))
            {
                return Convert.ToInt32(settingValue);
            }

            return defaultValue;
        }
    }
}
