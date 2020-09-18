// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;

    using DotNetNuke.Entities.Urls;

    public static class PortalAliasExtensions
    {
        public static bool ContainsAlias(this IEnumerable<PortalAliasInfo> aliases, int portalId, string httpAlias)
        {
            return aliases.Where(alias => alias.PortalID == portalId || portalId == -1)
                            .Any(alias => string.Compare(alias.HTTPAlias, httpAlias, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static bool ContainsSpecificSkins(this IEnumerable<PortalAliasInfo> aliases)
        {
            return aliases.Any(alias => !string.IsNullOrEmpty(alias.Skin));
        }

        public static Dictionary<string, string> GetAliasesAndCulturesForPortalId(this IEnumerable<PortalAliasInfo> aliases, int portalId)
        {
            var aliasCultures = new Dictionary<string, string>();
            foreach (var cpa in aliases)
            {
                if (aliasCultures.ContainsKey(cpa.HTTPAlias) == false)
                {
                    aliasCultures.Add(cpa.HTTPAlias.ToLowerInvariant(), cpa.CultureCode);
                }
            }

            return aliasCultures;
        }

        /// <summary>
        /// Returns the chosen portal alias for a specific portal Id and culture Code.
        /// </summary>
        /// <param name="aliases"></param>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <remarks>Detects the current browser type if possible.  If can't be deteced 'normal' is used. If a specific browser type is required, use overload with browser type.</remarks>
        /// <returns></returns>
        public static PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, int portalId, UrlAction result, string cultureCode, FriendlyUrlSettings settings)
        {
            var browserType = BrowserTypes.Normal;

            // if required, and possible, detect browser type
            if (HttpContext.Current != null && settings != null)
            {
                HttpRequest request = HttpContext.Current.Request;
                HttpResponse response = HttpContext.Current.Response;
                browserType = FriendlyUrlController.GetBrowserType(request, response, settings);
            }

            return GetAliasByPortalIdAndSettings(aliases, portalId, result, cultureCode, browserType);
        }

        public static PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, int portalId, string requestedAlias, string cultureCode, FriendlyUrlSettings settings)
        {
            var browserType = BrowserTypes.Normal;
            UrlAction result = null;

            // if required, and possible, detect browser type
            if (HttpContext.Current != null && settings != null)
            {
                HttpRequest request = HttpContext.Current.Request;
                HttpResponse response = HttpContext.Current.Response;
                browserType = FriendlyUrlController.GetBrowserType(request, response, settings);

                result = new UrlAction(HttpContext.Current.Request)
                {
                    IsSecureConnection = request.IsSecureConnection,
                    RawUrl = request.RawUrl,
                    HttpAlias = requestedAlias,
                };
            }

            return GetAliasByPortalIdAndSettings(aliases, portalId, result, cultureCode, browserType);
        }

        public static PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, UrlAction result)
        {
            return GetAliasByPortalIdAndSettings(aliases, result.PortalId, result, result.CultureCode, result.BrowserType);
        }

        /// <summary>
        /// Returns a ChosenPortalAlias object where the portalId, culture code and isMobile matches.
        /// </summary>
        /// <param name="aliases"></param>
        /// <param name="portalId"></param>
        /// <param name="result"></param>
        /// <param name="cultureCode"></param>
        /// <param name="browserType"></param>
        /// <returns>A ChosenPOrtalAlias.</returns>
        /// <remarks>Note will return a best-match by portal if no specific culture Code match found.</remarks>
        public static PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, int portalId, UrlAction result, string cultureCode, BrowserTypes browserType)
        {
            var aliasList = aliases.ToList();

            // First check if our current alias is already a perfect match.
            PortalAliasInfo foundAlias = null;
            if (result != null && !string.IsNullOrEmpty(result.HttpAlias))
            {
                // try to find exact match
                foundAlias = aliasList.FirstOrDefault(a => a.BrowserType == browserType &&
                                                         (string.Compare(a.CultureCode, cultureCode,
                                                             StringComparison.OrdinalIgnoreCase) == 0)
                                                         && a.PortalID == portalId
                                                         && a.HTTPAlias == result.HttpAlias);
                if (foundAlias == null) // let us try again using Startswith() to find matching Hosts
                {
                    foundAlias = aliasList.FirstOrDefault(a => a.BrowserType == browserType &&
                                                         (string.Compare(a.CultureCode, cultureCode,
                                                             StringComparison.OrdinalIgnoreCase) == 0)
                                                         && a.PortalID == portalId
                                                         && a.HTTPAlias.StartsWith(result.HttpAlias.Split('/')[0]));
                }
            }

            // 27138 : Redirect loop caused by duplicate primary aliases.  Changed to only check by browserType/Culture code which makes a primary alias
            if (foundAlias == null)
            {
                foundAlias = aliasList.Where(a => a.BrowserType == browserType
                                            && (string.Compare(a.CultureCode, cultureCode, StringComparison.OrdinalIgnoreCase) == 0 || string.IsNullOrEmpty(a.CultureCode))
                                            && a.PortalID == portalId)
                    .OrderByDescending(a => a.IsPrimary)
                    .ThenByDescending(a => a.CultureCode)
                    .FirstOrDefault();
            }

            // JIRA DNN-4882 : DevPCI fix bug with url Mobile -> Search alias with culture code
            // START DNN-4882
            if (foundAlias == null)
            {
                foundAlias = aliasList.Where(a => (string.Compare(a.CultureCode, cultureCode, StringComparison.OrdinalIgnoreCase) == 0 || string.IsNullOrEmpty(a.CultureCode))
                                           && a.PortalID == portalId)
                       .OrderByDescending(a => a.IsPrimary)
                       .ThenByDescending(a => a.CultureCode)
                       .FirstOrDefault();
            }

            // END DNN-4882
            if (foundAlias != null)
            {
                if (result != null && result.PortalAlias != null)
                {
                    if (foundAlias.BrowserType != result.PortalAlias.BrowserType)
                    {
                        result.Reason = foundAlias.CultureCode != result.PortalAlias.CultureCode
                            ? RedirectReason.Wrong_Portal_Alias_For_Culture_And_Browser
                            : RedirectReason.Wrong_Portal_Alias_For_Browser_Type;
                    }
                    else
                    {
                        if (foundAlias.CultureCode != result.PortalAlias.CultureCode)
                        {
                            result.Reason = RedirectReason.Wrong_Portal_Alias_For_Culture;
                        }
                    }
                }
            }
            else
            {
                // if we didn't find a specific match, return the default, which is the closest match
                var defaultAlias = aliasList
                    .Where(a => a.PortalID == portalId)
                    .OrderByDescending(a => a.IsPrimary)
                    .FirstOrDefault();

                foundAlias = defaultAlias;
            }

            return foundAlias;
        }

        public static List<string> GetAliasesForPortalId(this IEnumerable<PortalAliasInfo> aliases, int portalId)
        {
            var httpAliases = new List<string>();
            foreach (var cpa in aliases.Where(cpa => httpAliases.Contains(cpa.HTTPAlias) == false))
            {
                httpAliases.Add(cpa.HTTPAlias.ToLowerInvariant());
            }

            return httpAliases;
        }

        public static string GetCultureByPortalIdAndAlias(this IEnumerable<PortalAliasInfo> aliases, int portalId, string alias)
        {
            return (from cpa in aliases
                    where cpa.PortalID == portalId && string.Compare(alias, cpa.HTTPAlias, StringComparison.OrdinalIgnoreCase) == 0
                    select cpa.CultureCode)
                    .FirstOrDefault();
        }

        public static void GetSettingsByPortalIdAndAlias(this IEnumerable<PortalAliasInfo> aliases, int portalId, string alias, out string culture, out BrowserTypes browserType, out string skin)
        {
            culture = null;
            browserType = BrowserTypes.Normal;
            skin = string.Empty;
            foreach (var cpa in aliases)
            {
                if (cpa.PortalID == portalId && string.Compare(alias, cpa.HTTPAlias, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // this is a match
                    culture = cpa.CultureCode;
                    browserType = cpa.BrowserType;

                    // 852 : add skin per portal alias
                    skin = cpa.Skin;
                    break;
                }
            }
        }
    }
}
