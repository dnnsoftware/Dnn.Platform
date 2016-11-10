using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Pages.Components
{
    public class PageUrlsController : ServiceLocator<IPageUrlsController, PageUrlsController>, IPageUrlsController
    {
        private enum SortingFields { None = 0, Url, Locale, Status };

        public IEnumerable<Url> GetPageUrls(TabInfo tab, int portalId)
        {
            
            var locales = new Lazy<Dictionary<string, Locale>>(() => LocaleController.Instance.GetLocales(portalId));
            var customUrls = GetSortedUrls(tab, portalId, locales, 1, true, false);
            var automaticUrls = GetSortedUrls(tab, portalId, locales, 1, true, true).ToList();

            automaticUrls.AddRange(customUrls);
            return automaticUrls;
        }

        private IEnumerable<Url> GetSortedUrls(TabInfo tab, int portalId, Lazy<Dictionary<string, Locale>> locales, int sortColumn, bool sortOrder, bool isSystem)
        {
            var friendlyUrlSettings = new FriendlyUrlSettings(tab.PortalID);
            var tabs = new List<Url>();

            if (isSystem)
            {
                //Add generated urls
                foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId))
                {
                    var urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == alias.CultureCode);

                    /*var isRedirected = tab.TabUrls.Any(u => u.HttpStatus == "200"
                                                            && u.CultureCode == ((urlLocale != null) ? urlLocale.Code : String.Empty))
                                            || alias.PortalAliasID != PrimaryAliasId;*/

                    bool isRedirected = false;
                    var isCustom200Urls = tab.TabUrls.Any(u => u.HttpStatus == "200");//are there any custom Urls for this tab?
                    var baseUrl = Globals.AddHTTP(alias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
                    if (urlLocale != null) baseUrl += "&language=" + urlLocale.Code;
                    string customPath = null;
                    if (isCustom200Urls)
                    {
                        //get the friendlyUrl, including custom Urls
                        customPath = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab,
                                                                                baseUrl,
                                                                                Globals.glbDefaultPage,
                                                                                alias.HTTPAlias,
                                                                                false,
                                                                                friendlyUrlSettings,
                                                                                Guid.Empty);

                        customPath = customPath.Replace(Globals.AddHTTP(alias.HTTPAlias), "");
                    }
                    //get the friendlyUrl and ignore and custom Urls
                    var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab,
                                                                                baseUrl,
                                                                                Globals.glbDefaultPage,
                                                                                alias.HTTPAlias,
                                                                                true,
                                                                                friendlyUrlSettings,
                                                                                Guid.Empty);

                    path = path.Replace(Globals.AddHTTP(alias.HTTPAlias), "");
                    int status = 200;
                    if (customPath != null && (string.Compare(customPath, path, StringComparison.InvariantCultureIgnoreCase) != 0))
                    {
                        //difference in custom/standard URL, so standard is 301
                        status = 301;
                        isRedirected = true;
                    }
                    //AddUrlToList(tabs, -1, alias, urlLocale, path, String.Empty, (isRedirected) ? 301 : 200);
                    //27139 : only show primary aliases in the tab grid (gets too confusing otherwise)
                    if (alias.IsPrimary) //alias was provided to FriendlyUrlCall, so will always get the correct canonical Url back
                        AddUrlToList(tabs, -1, alias, urlLocale, path, String.Empty, status, isSystem, friendlyUrlSettings);

                    //Add url with diacritics
                    isRedirected = friendlyUrlSettings.RedirectUnfriendly;
                    bool replacedDiacritic;
                    string asciiTabPath = TabPathHelper.ReplaceDiacritics(tab.TabPath, out replacedDiacritic).Replace("//", "/");
                    if (replacedDiacritic)
                    {
                        if (friendlyUrlSettings.AutoAsciiConvert)
                        {
                            if (friendlyUrlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                            {
                                path = path.Replace(friendlyUrlSettings.ReplaceSpaceWith, String.Empty);
                            }
                            path = path.Replace(asciiTabPath, tab.TabPath.Replace("//", "/"));
                            AddUrlToList(tabs, -1, alias, urlLocale, path, String.Empty, (isRedirected) ? 301 : 200, isSystem, friendlyUrlSettings);
                        }
                    }
                    else
                    {
                        //Add url with space
                        if (tab.TabName.Contains(" ") && friendlyUrlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                        {
                            path = path.Replace(friendlyUrlSettings.ReplaceSpaceWith, String.Empty);
                            if (customPath != null && string.Compare(customPath, path, StringComparison.InvariantCultureIgnoreCase) != 0)
                            {
                                AddUrlToList(tabs, -1, alias, urlLocale, path, String.Empty, (isRedirected) ? 301 : 200, isSystem, friendlyUrlSettings);
                            }
                        }

                    }
                }
            }

            foreach (var url in tab.TabUrls.Where(u => u.IsSystem == isSystem).OrderBy(u => u.SeqNum))
            {
                var statusCode = Int32.Parse(url.HttpStatus);

                //27133 : Only show a custom URL 
                if (url.PortalAliasUsage == PortalAliasUsageType.Default)
                {
                    var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
                    var alias = aliases.FirstOrDefault(primary => primary.IsPrimary == true);
                    if (alias == null)
                    {
                        //if no primary alias just get first in list, need to use something
                        alias = aliases.FirstOrDefault(a => a.PortalID == portalId);
                    }
                    if (alias != null)
                    {
                        var urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == alias.CultureCode);
                        AddUrlToList(tabs, url.SeqNum, alias, urlLocale, url.Url, url.QueryString, statusCode, isSystem, friendlyUrlSettings);
                    }
                }
                else
                {
                    var urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == url.CultureCode);
                    var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId)
                        .SingleOrDefault(p => p.PortalAliasID == url.PortalAliasId);

                    AddUrlToList(tabs, url.SeqNum, alias, urlLocale, url.Url, url.QueryString, statusCode, isSystem, friendlyUrlSettings);
                }
            }

            var pairComparer = new KeyValuePairComparer();
            switch ((SortingFields)sortColumn)
            {
                case SortingFields.Url:
                case SortingFields.None:
                    return sortOrder ?
                        tabs.OrderBy(url => url.SiteAlias, pairComparer).ThenBy(url => url.Path) :
                        tabs.OrderByDescending(url => url.SiteAlias, pairComparer).ThenByDescending(url => url.Path);
                case SortingFields.Locale:
                    return sortOrder ?
                        tabs.OrderBy(url => url.Locale, pairComparer) :
                        tabs.OrderByDescending(url => url.Locale, pairComparer);
                case SortingFields.Status:
                    return sortOrder ?
                        tabs.OrderBy(url => url.StatusCode, pairComparer) :
                        tabs.OrderByDescending(url => url.StatusCode, pairComparer);
                default:
                    return sortOrder ?
                        tabs.OrderBy(url => url.SiteAlias, pairComparer).ThenBy(url => url.Path) :
                        tabs.OrderByDescending(url => url.SiteAlias, pairComparer).ThenByDescending(url => url.Path);
            }
        }

        private void AddUrlToList(List<Url> tabs, int id, PortalAliasInfo alias, Locale urlLocale, string path, string queryString, int statusCode, bool isSystem, FriendlyUrlSettings friendlyUrlSettings)
        {
            tabs.Add(new Url
            {
                Id = id,
                SiteAlias = new KeyValuePair<int, string>(alias.KeyID, alias.HTTPAlias),
                Path = path,
                PathWithNoExtension = GetCleanPath(path, friendlyUrlSettings),
                QueryString = queryString,
                Locale = (urlLocale != null) ? new KeyValuePair<int, string>(urlLocale.KeyID, urlLocale.EnglishName)
                                             : new KeyValuePair<int, string>(-1, ""),
                StatusCode = StatusCodes.SingleOrDefault(kv => kv.Key == statusCode),
                SiteAliasUsage = (int)PortalAliasUsageType.ChildPagesInherit,
                IsSystem = isSystem
            });
        }

        protected IEnumerable<KeyValuePair<int, string>> StatusCodes
        {
            get
            {
                return new[]
                {
                    new KeyValuePair<int, string>(200, "Active (200)"),
                    new KeyValuePair<int, string>(301, "Redirect (301)")
                };
            }
        }
        private string GetCleanPath(string path, FriendlyUrlSettings friendlyUrlSettings)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            var urlPath = path.TrimStart('/');
            urlPath = UrlRewriterUtils.CleanExtension(urlPath, friendlyUrlSettings, string.Empty);

            return string.Format("/{0}", urlPath);
        }
        public class KeyValuePairComparer : IComparer<KeyValuePair<int, string>>
        {
            public int Compare(KeyValuePair<int, string> pair1, KeyValuePair<int, string> pair2)
            {
                return String.Compare(pair1.Value, pair2.Value, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        protected override Func<IPageUrlsController> GetFactory()
        {
            return () => new PageUrlsController();
        }
    }
}