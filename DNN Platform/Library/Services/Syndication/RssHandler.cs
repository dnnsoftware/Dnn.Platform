// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An HTTP handler for serving an RSS feed.</summary>
    public class RssHandler : SyndicationHandlerBase
    {
        private readonly ISearchController searchController;

        /// <summary>Initializes a new instance of the <see cref="RssHandler"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with ISearchController. Scheduled removal in v12.0.0.")]
        public RssHandler()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RssHandler"/> class.</summary>
        /// <param name="searchController">The search controller.</param>
        public RssHandler(ISearchController searchController)
        {
            this.searchController = searchController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ISearchController>();
        }

        /// <inheritdoc />
        protected override void PopulateChannel(string channelName, string userName)
        {
            if (this.Request == null || this.Settings?.ActiveTab == null || this.ModuleId == Null.NullInteger)
            {
                return;
            }

            this.Channel["title"] = this.Settings.PortalName;
            this.Channel["link"] = Globals.AddHTTP(Globals.GetDomainName(this.Request));
            if (!string.IsNullOrEmpty(this.Settings.Description))
            {
                this.Channel["description"] = this.Settings.Description;
            }
            else
            {
                this.Channel["description"] = this.Settings.PortalName;
            }

            this.Channel["language"] = this.Settings.DefaultLanguage;
            this.Channel["copyright"] = !string.IsNullOrEmpty(this.Settings.FooterText) ? this.Settings.FooterText.Replace("[year]", DateTime.Now.Year.ToString(CultureInfo.CurrentCulture)) : string.Empty;
            this.Channel["webMaster"] = this.Settings.Email;

            IList<SearchResult> searchResults = null;
            var query = new SearchQuery
            {
                PortalIds = [this.Settings.PortalId,],
                TabId = this.TabId,
                ModuleId = this.ModuleId,
                SearchTypeIds = [SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,],
            };

            try
            {
                searchResults = this.searchController.ModuleSearch(query).Results;
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }

            if (searchResults != null)
            {
                foreach (var result in searchResults)
                {
                    if (!result.UniqueKey.StartsWith(Constants.ModuleMetaDataPrefixTag, StringComparison.Ordinal) && TabPermissionController.CanViewPage())
                    {
                        if (this.Settings.ActiveTab.StartDate < DateTime.Now && this.Settings.ActiveTab.EndDate > DateTime.Now)
                        {
                            var objModule = ModuleController.Instance.GetModule(result.ModuleId, query.TabId, false);
                            if (objModule is { DisplaySyndicate: true, IsDeleted: false, })
                            {
                                if (ModulePermissionController.CanViewModule(objModule))
                                {
                                    if (Convert.ToDateTime(objModule.StartDate == Null.NullDate ? DateTime.MinValue : objModule.StartDate) < DateTime.Now &&
                                        Convert.ToDateTime(objModule.EndDate == Null.NullDate ? DateTime.MaxValue : objModule.EndDate) > DateTime.Now)
                                    {
                                        this.Channel.Items.Add(GetRssItem(result));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The PreRender event is used to set the Caching Policy for the Feed.  This mimics the behavior from the
        /// OutputCache directive in the old Rss.aspx file.  @OutputCache Duration="60" VaryByParam="moduleid".
        /// </summary>
        /// <param name="ea">Event Args.</param>
        protected override void OnPreRender(EventArgs ea)
        {
            base.OnPreRender(ea);

            this.Context.Response.Cache.SetExpires(DateTime.Now.AddSeconds(60));
            this.Context.Response.Cache.SetCacheability(HttpCacheability.Public);
            this.Context.Response.Cache.VaryByParams["moduleid"] = true;
        }

        /// <summary>Creates an RSS Item.</summary>
        /// <param name="searchResult">The search result to convert to an RSS item.</param>
        /// <returns>A new <see cref="GenericRssElement"/> instance.</returns>
        private static GenericRssElement GetRssItem(SearchResult searchResult)
        {
            var item = new GenericRssElement();
            var url = searchResult.Url;
            if (url.Trim() == string.Empty)
            {
                url = TestableGlobals.Instance.NavigateURL(searchResult.TabId);
                if (url.IndexOf(HttpContext.Current.Request.Url.Host, StringComparison.InvariantCultureIgnoreCase) == -1)
                {
                    url = TestableGlobals.Instance.AddHTTP(HttpContext.Current.Request.Url.Host) + url;
                }
            }

            item["title"] = searchResult.Title;
            item["description"] = searchResult.Description;
            item["pubDate"] = searchResult.ModifiedTimeUtc.ToUniversalTime().ToString("r");
            item["link"] = url;
            item["guid"] = url;

            return item;
        }
    }
}
