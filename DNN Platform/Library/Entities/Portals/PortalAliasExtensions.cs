// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Internal.SourceGenerators;

    using NewBrowserTypes = DotNetNuke.Abstractions.Urls.BrowserTypes;
#pragma warning disable CS0618 // Type or member is obsolete
    using OldBrowserTypes = DotNetNuke.Entities.Urls.BrowserTypes;
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>Extensions for portal aliases.</summary>
    public static partial class PortalAliasExtensions
    {
        public static bool ContainsAlias(this IEnumerable<PortalAliasInfo> aliases, int portalId, string httpAlias)
        {
            return aliases.Cast<IPortalAliasInfo>()
                .Where(alias => alias.PortalId == portalId || portalId == -1)
                .Any(alias => string.Equals(alias.HttpAlias, httpAlias, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ContainsSpecificSkins(this IEnumerable<PortalAliasInfo> aliases)
        {
            return aliases.Any(alias => !string.IsNullOrEmpty(alias.Skin));
        }

        public static Dictionary<string, string> GetAliasesAndCulturesForPortalId(this IEnumerable<PortalAliasInfo> aliases, int portalId)
        {
            var aliasCultures = new Dictionary<string, string>();
            foreach (IPortalAliasInfo cpa in aliases)
            {
                if (!aliasCultures.ContainsKey(cpa.HttpAlias))
                {
                    aliasCultures.Add(cpa.HttpAlias.ToLowerInvariant(), cpa.CultureCode);
                }
            }

            return aliasCultures;
        }

        /// <summary>Returns the chosen portal alias for a specific portal ID and culture Code.</summary>
        /// <param name="aliases">The aliases.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="result">The URL action.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <remarks>Detects the current browser type if possible.  If it can't be detected <see cref="BrowserTypes.Normal"/> is used. If a specific browser type is required, use overload with browser type.</remarks>
        /// <returns>The closest <see cref="PortalAliasInfo"/> match.</returns>
        public static PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, int portalId, UrlAction result, string cultureCode, FriendlyUrlSettings settings)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var browserType = OldBrowserTypes.Normal;
#pragma warning restore CS0618 // Type or member is obsolete

            // if required, and possible, detect browser type
            if (HttpContext.Current != null && settings != null)
            {
                HttpRequest request = HttpContext.Current.Request;
                HttpResponse response = HttpContext.Current.Response;

                // TODO: create version of GetBrowserType that returns NewBrowserTypes
                browserType = FriendlyUrlController.GetBrowserType(request, response, settings);
            }

            return GetAliasByPortalIdAndSettings(aliases, portalId, result, cultureCode, browserType.ToAbstractionsBrowserTypes());
        }

        public static PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, int portalId, string requestedAlias, string cultureCode, FriendlyUrlSettings settings)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var browserType = OldBrowserTypes.Normal;
#pragma warning restore CS0618 // Type or member is obsolete
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

            return GetAliasByPortalIdAndSettings(aliases, portalId, result, cultureCode, browserType.ToAbstractionsBrowserTypes());
        }

        public static PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, UrlAction result)
        {
            return GetAliasByPortalIdAndSettings(aliases, result.PortalId, result, result.CultureCode, result.BrowserType.ToAbstractionsBrowserTypes());
        }

        /// <summary>Returns a ChosenPortalAlias object where the portalId, culture code and isMobile matches.</summary>
        /// <param name="aliases">The aliases.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="result">The URL action.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="browserType">The browser type.</param>
        /// <returns>The closest <see cref="PortalAliasInfo"/> match.</returns>
        /// <remarks>Note will return a best-match by portal if no specific culture Code match found.</remarks>
        [DnnDeprecated(10, 2, 2, "Use overload taking DotNetNuke.Abstractions.Urls.BrowserTypes")]
        public static partial PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, int portalId, UrlAction result, string cultureCode, OldBrowserTypes browserType)
            => aliases.GetAliasByPortalIdAndSettings(portalId, result, cultureCode, browserType.ToAbstractionsBrowserTypes());

        /// <summary>Returns a ChosenPortalAlias object where the portalId, culture code and isMobile matches.</summary>
        /// <param name="aliases">The aliases.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="result">The URL action.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="browserType">The browser type.</param>
        /// <returns>The closest <see cref="PortalAliasInfo"/> match.</returns>
        /// <remarks>Note will return a best-match by portal if no specific culture Code match found.</remarks>
        public static PortalAliasInfo GetAliasByPortalIdAndSettings(this IEnumerable<PortalAliasInfo> aliases, int portalId, UrlAction result, string cultureCode, NewBrowserTypes browserType)
        {
            var aliasList = aliases.ToList();

            // First check if our current alias is already a perfect match.
            PortalAliasInfo foundAlias = null;
            if (result != null && !string.IsNullOrEmpty(result.HttpAlias))
            {
                // try to find exact match
                foundAlias = aliasList.FirstOrDefault(a =>
                    ((IPortalAliasInfo)a).BrowserType == browserType &&
                    string.Equals(a.CultureCode, cultureCode, StringComparison.OrdinalIgnoreCase) &&
                    ((IPortalAliasInfo)a).PortalId == portalId &&
                    ((IPortalAliasInfo)a).HttpAlias == result.HttpAlias);
                if (foundAlias == null)
                {
                    // let us try again using StartsWith() to find matching Hosts
                    foundAlias = aliasList.FirstOrDefault(a =>
                        ((IPortalAliasInfo)a).BrowserType == browserType &&
                        string.Equals(a.CultureCode, cultureCode, StringComparison.OrdinalIgnoreCase) &&
                        ((IPortalAliasInfo)a).PortalId == portalId &&
                        ((IPortalAliasInfo)a).HttpAlias.StartsWith(result.HttpAlias.Split('/')[0], StringComparison.OrdinalIgnoreCase));
                }
            }

            // 27138 : Redirect loop caused by duplicate primary aliases.  Changed to only check by browserType/Culture code which makes a primary alias
            if (foundAlias == null)
            {
                foundAlias = aliasList.Where(
                        a => ((IPortalAliasInfo)a).BrowserType == browserType
                             && (string.Equals(a.CultureCode, cultureCode, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(a.CultureCode))
                             && ((IPortalAliasInfo)a).PortalId == portalId)
                    .OrderByDescending(a => a.IsPrimary)
                    .ThenByDescending(a => a.CultureCode)
                    .FirstOrDefault();
            }

            // JIRA DNN-4882 : DevPCI fix bug with url Mobile -> Search alias with culture code
            // START DNN-4882
            if (foundAlias == null)
            {
                foundAlias = aliasList.Where(
                        a => (string.Equals(a.CultureCode, cultureCode, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(a.CultureCode))
                             && ((IPortalAliasInfo)a).PortalId == portalId)
                    .OrderByDescending(a => a.IsPrimary)
                    .ThenByDescending(a => a.CultureCode)
                    .FirstOrDefault();
            }

            // END DNN-4882
            if (foundAlias != null)
            {
                if (result is { PortalAlias: not null })
                {
                    if (((IPortalAliasInfo)foundAlias).BrowserType != ((IPortalAliasInfo)result.PortalAlias).BrowserType)
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
                    .Where(a => ((IPortalAliasInfo)a).PortalId == portalId)
                    .OrderByDescending(a => a.IsPrimary)
                    .FirstOrDefault();

                foundAlias = defaultAlias;
            }

            return foundAlias;
        }

        public static List<string> GetAliasesForPortalId(this IEnumerable<PortalAliasInfo> aliases, int portalId)
        {
            var httpAliases = new List<string>();
            foreach (var cpa in aliases.Where((IPortalAliasInfo cpa) => !httpAliases.Contains(cpa.HttpAlias)))
            {
                httpAliases.Add(cpa.HttpAlias.ToLowerInvariant());
            }

            return httpAliases;
        }

        public static string GetCultureByPortalIdAndAlias(this IEnumerable<PortalAliasInfo> aliases, int portalId, string alias)
        {
            return (from IPortalAliasInfo portalAlias in aliases
                    where portalAlias.PortalId == portalId && string.Equals(alias, portalAlias.HttpAlias, StringComparison.OrdinalIgnoreCase)
                    select portalAlias.CultureCode)
                .FirstOrDefault();
        }

        /// <summary>Gets the culture, browser type, and skin for the given <paramref name="portalId"/> and <paramref name="alias"/>.</summary>
        /// <param name="aliases">The aliases.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="browserType">The browser type.</param>
        /// <param name="skin">The skin.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking DotNetNuke.Abstractions.Urls.BrowserTypes")]
        public static partial void GetSettingsByPortalIdAndAlias(this IEnumerable<PortalAliasInfo> aliases, int portalId, string alias, out string culture, out OldBrowserTypes browserType, out string skin)
        {
            aliases.GetSettingsByPortalIdAndAlias(portalId, alias, out culture, out NewBrowserTypes newBrowserType, out skin);
            browserType = newBrowserType.ToDeprecatedBrowserTypes();
        }

        /// <summary>Gets the culture, browser type, and skin for the given <paramref name="portalId"/> and <paramref name="alias"/>.</summary>
        /// <param name="aliases">The aliases.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="browserType">The browser type.</param>
        /// <param name="skin">The skin.</param>
        public static void GetSettingsByPortalIdAndAlias(this IEnumerable<PortalAliasInfo> aliases, int portalId, string alias, out string culture, out NewBrowserTypes browserType, out string skin)
        {
            culture = null;
            browserType = NewBrowserTypes.Normal;
            skin = string.Empty;
            foreach (IPortalAliasInfo portalAlias in aliases)
            {
                if (portalAlias.PortalId == portalId && string.Equals(alias, portalAlias.HttpAlias, StringComparison.OrdinalIgnoreCase))
                {
                    // this is a match
                    culture = portalAlias.CultureCode;
                    browserType = portalAlias.BrowserType;

                    // 852 : add skin per portal alias
                    skin = portalAlias.Skin;
                    break;
                }
            }
        }
    }
}
