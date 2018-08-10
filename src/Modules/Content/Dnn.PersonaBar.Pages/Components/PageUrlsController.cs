using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dnn.PersonaBar.Pages.Components.Dto;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
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
            return automaticUrls.OrderBy(url => url.StatusCode, new KeyValuePairComparer()).ThenBy(url => url.Path);
        }

        public PageUrlResult CreateCustomUrl(SaveUrlDto dto, TabInfo tab)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            PortalInfo aliasPortal = new PortalAliasController().GetPortalByPortalAliasID(dto.SiteAliasKey);

            if (aliasPortal != null && portalSettings.PortalId != aliasPortal.PortalID)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPortalAlias.Error"),
                    SuggestedUrlPath = String.Empty
                    };
            }

            var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
            bool modified;
            //Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalSettings.PortalId)));

            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                return new PageUrlResult {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error"),
                    SuggestedUrlPath = "/" + urlPath
                };
            }

            //Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, portalSettings, out modified);
            if (modified)
            {
                return new PageUrlResult {
                    Success = false,
                    ErrorMessage = Localization.GetString("UrlPathNotUnique.Error"),
                    SuggestedUrlPath = "/" + urlPath
                };
            }

            if (tab.TabUrls.Any(u => u.Url.ToLowerInvariant() == dto.Path.ValueOrEmpty().ToLowerInvariant()
                                     && (u.PortalAliasId == dto.SiteAliasKey || u.PortalAliasId == -1)))
            {
                return new PageUrlResult {
                    Success = false,
                    ErrorMessage = Localization.GetString("DuplicateUrl.Error")
                };
            }

            var seqNum = (tab.TabUrls.Count > 0) ? tab.TabUrls.Max(t => t.SeqNum) + 1 : 1;
            var portalLocales = LocaleController.Instance.GetLocales(portalSettings.PortalId);
            var cultureCode = portalLocales.Where(l => l.Value.KeyID == dto.LocaleKey)
                                .Select(l => l.Value.Code)
                                .SingleOrDefault() ?? portalSettings.CultureCode;

            var portalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
            if (portalAliasUsage == PortalAliasUsageType.Default)
            {
                var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalSettings.PortalId)
                                                        .SingleOrDefault(a => a.PortalAliasID == dto.SiteAliasKey);

                if (string.IsNullOrEmpty(cultureCode) || alias == null)
                {
                    return new PageUrlResult
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("InvalidRequest.Error")
                    };
                }
            }
            else
            {
                var cultureAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalSettings.PortalId)
                                                            .FirstOrDefault(a => a.CultureCode == cultureCode);

                if (portalLocales.Count > 1 && !portalSettings.ContentLocalizationEnabled && (string.IsNullOrEmpty(cultureCode) || cultureAlias == null))
                {
                    return new PageUrlResult
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("InvalidRequest.Error")
                    };
                }
            }

            var tabUrl = new TabUrlInfo
            {
                TabId = tab.TabID,
                SeqNum = seqNum,
                PortalAliasId = dto.SiteAliasKey,
                PortalAliasUsage = portalAliasUsage,
                QueryString = dto.QueryString.ValueOrEmpty(),
                Url = dto.Path.ValueOrEmpty(),
                CultureCode = cultureCode,
                HttpStatus = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture),
                IsSystem = false
            };

            TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);

            return new PageUrlResult
            {
                Success = true,
                Id = seqNum // returns Id of the created Url
            };
        }

        public PageUrlResult UpdateCustomUrl(SaveUrlDto dto, TabInfo tab)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
            bool modified;
            //Clean Url
            var options =
                UrlRewriterUtils.ExtendOptionsForCustomURLs(
                    UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalSettings.PortalId)));

            //now clean the path
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error"),
                    SuggestedUrlPath = "/" + urlPath
                };
            }

            //Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, tab.TabID, portalSettings, out modified);
            if (modified)
            {
                return new PageUrlResult
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("UrlPathNotUnique.Error"),
                    SuggestedUrlPath = "/" + urlPath
                };
            }

            var cultureCode = LocaleController.Instance.GetLocales(portalSettings.PortalId)
                .Where(l => l.Value.KeyID == dto.LocaleKey)
                .Select(l => l.Value.Code)
                .SingleOrDefault() ?? portalSettings.DefaultLanguage;

            var statusCodeKey = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture);
            var tabUrl = tab.TabUrls.SingleOrDefault(t => t.SeqNum == dto.Id && t.HttpStatus == statusCodeKey);

            if (statusCodeKey == "200")
            {
                //We need to check if we are updating a current url or creating a new 200                
                if (tabUrl == null)
                {
                    //Just create Url
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = dto.Id,
                        PortalAliasId = dto.SiteAliasKey,
                        PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage,
                        QueryString = dto.QueryString.ValueOrEmpty(),
                        Url = dto.Path.ValueOrEmpty(),
                        CultureCode = cultureCode,
                        HttpStatus = "200",
                        IsSystem = dto.IsSystem // false
                    };
                    TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    if (!tabUrl.Url.Equals("/" + urlPath, StringComparison.OrdinalIgnoreCase))
                    {
                        //Change the original 200 url to a redirect
                        tabUrl.HttpStatus = "301";
                        tabUrl.SeqNum = dto.Id;
                        TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);

                        //Add new custom url
                        tabUrl.Url = dto.Path.ValueOrEmpty();
                        tabUrl.HttpStatus = "200";
                        tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                        tabUrl.CultureCode = cultureCode;
                        tabUrl.PortalAliasId = dto.SiteAliasKey;
                        tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                        tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                        TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    }
                    else
                    {
                        //Update the original 200 url
                        tabUrl.CultureCode = cultureCode;
                        tabUrl.PortalAliasId = dto.SiteAliasKey;
                        tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                        tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                        TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                    }
                }
            }
            else
            {
                //Update the original non 200 url
                if (tabUrl == null)
                {
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = dto.Id,
                        PortalAliasId = dto.SiteAliasKey,
                        PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage,
                        QueryString = dto.QueryString.ValueOrEmpty(),
                        Url = dto.Path.ValueOrEmpty(),
                        CultureCode = cultureCode,
                        HttpStatus = statusCodeKey,
                        IsSystem = dto.IsSystem // false
                    };
                    TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    tabUrl.CultureCode = cultureCode;
                    tabUrl.PortalAliasId = dto.SiteAliasKey;
                    tabUrl.PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage;
                    tabUrl.Url = dto.Path.ValueOrEmpty();
                    tabUrl.HttpStatus = statusCodeKey;
                    tabUrl.QueryString = dto.QueryString.ValueOrEmpty();
                    TabController.Instance.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
            }


            return new PageUrlResult
            {
                Success = true
            };
        }

        public PageUrlResult DeleteCustomUrl(int id, TabInfo tab)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tabUrl = tab.TabUrls.SingleOrDefault(u => u.SeqNum == id);

            TabController.Instance.DeleteTabUrl(tabUrl, portalSettings.PortalId, true);

            return new PageUrlResult
            {
                Success = true
            };
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
                    if (customPath != null && (string.Compare(customPath, path, StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        //difference in custom/standard URL, so standard is 301
                        status = 301;
                        isRedirected = true;
                    }
                    //AddUrlToList(tabs, -1, alias, urlLocale, path, String.Empty, (isRedirected) ? 301 : 200);
                    //27139 : only show primary aliases in the tab grid (gets too confusing otherwise)
                    if (alias.IsPrimary) //alias was provided to FriendlyUrlCall, so will always get the correct canonical Url back
                        AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, String.Empty, status, isSystem, friendlyUrlSettings, null);

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
                            AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, String.Empty, (isRedirected) ? 301 : 200, isSystem, friendlyUrlSettings, null);
                        }
                    }
                    else
                    {
                        //Add url with space
                        if (tab.TabName.Contains(" ") && friendlyUrlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                        {
                            path = path.Replace(friendlyUrlSettings.ReplaceSpaceWith, String.Empty);
                            if (customPath != null && string.Compare(customPath, path, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                AddUrlToList(tabs, portalId, -1, alias, urlLocale, path, String.Empty, (isRedirected) ? 301 : 200, isSystem, friendlyUrlSettings, null);
                            }
                        }

                    }
                }
            }

            foreach (var url in tab.TabUrls.Where(u => u.IsSystem == isSystem).OrderBy(u => u.SeqNum))
            {
                int statusCode;
                int.TryParse(url.HttpStatus, out statusCode);

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
                        AddUrlToList(tabs, portalId, url.SeqNum, alias, urlLocale, url.Url, url.QueryString, statusCode, isSystem, friendlyUrlSettings, url.LastModifiedByUserId);
                    }
                }
                else
                {
                    var urlLocale = locales.Value.Values.FirstOrDefault(local => local.Code == url.CultureCode);
                    var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId)
                        .SingleOrDefault(p => p.PortalAliasID == url.PortalAliasId);
                    if (alias != null)
                    {
                        AddUrlToList(tabs, portalId, url.SeqNum, alias, urlLocale, url.Url, url.QueryString, statusCode, isSystem, friendlyUrlSettings, url.LastModifiedByUserId);
                    }
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

        private void AddUrlToList(List<Url> tabs, int portalId, int id, PortalAliasInfo alias, Locale urlLocale, string path, string queryString, int statusCode, bool isSystem, FriendlyUrlSettings friendlyUrlSettings, int? lastModifiedByUserId)
        {
            var userName = "";
            if (lastModifiedByUserId.HasValue)
            {
                userName = UserController.Instance.GetUser(portalId, lastModifiedByUserId.Value)?.DisplayName;
            }

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
                IsSystem = isSystem,
                UserName = userName
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
                return String.Compare(pair1.Value, pair2.Value, StringComparison.OrdinalIgnoreCase);
            }
        }

        protected override Func<IPageUrlsController> GetFactory()
        {
            return () => new PageUrlsController();
        }
    }
}