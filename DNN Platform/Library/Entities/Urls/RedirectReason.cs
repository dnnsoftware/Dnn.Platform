// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
#pragma warning disable CA1707 // Identifiers should not contain underscores
    public enum RedirectReason
    {
        /// <summary>The URL was in an unfriendly format.</summary>
        Unfriendly_Url_1 = 0,

        /// <summary>The URL was in an unfriendly format.</summary>
        Unfriendly_Url_2 = 1,

        /// <summary>The URL was not in lower case.</summary>
        Not_Lower_Case = 2,

        /// <summary>The URL was for the wrong subdomain.</summary>
        Wrong_Sub_Domain = 3,

        /// <summary>The URL matched a custom redirect.</summary>
        Custom_Redirect = 4,

        /// <summary>The URL is for the wrong portal.</summary>
        Wrong_Portal = 5,

        /// <summary>The URL was not redirected.</summary>
        Not_Redirected = 6,

        /// <summary>The URL was for a deleted page.</summary>
        Deleted_Page = 7,

        /// <summary>The URL was for a disabled page.</summary>
        Disabled_Page = 8,

        /// <summary>The URL had spaces replaced.</summary>
        Spaces_Replaced = 9,

        /// <summary>The URL was for the wrong portal alias.</summary>
        Wrong_Portal_Alias = 10,

        /// <summary>The URL was for the wrong portal alias due to the browser type.</summary>
        Wrong_Portal_Alias_For_Browser_Type = 11,

        /// <summary>The URL was for the wrong portal alias due to the culture.</summary>
        Wrong_Portal_Alias_For_Culture = 12,

        /// <summary>The URL was for the wrong portal alias due to both the culture and the browser type.</summary>
        Wrong_Portal_Alias_For_Culture_And_Browser = 13,

        /// <summary>The tab has a custom alias.</summary>
        Custom_Tab_Alias = 14,

        /// <summary>The URL is for the root of the site.</summary>
        Site_Root_Home = 15,

        /// <summary>The URL is for a secure page.</summary>
        Secure_Page_Requested = 16,

        /// <summary>The page has an external URL.</summary>
        Tab_External_Url = 17,

        /// <summary>The page has a permanent redirect.</summary>
        Tab_Permanent_Redirect = 18,

        /// <summary>The host portal was used.</summary>
        Host_Portal_Used = 19,

        /// <summary>The URL contained an alias.</summary>
        Alias_In_Url = 20,

        /// <summary>The URL was for a user's profile.</summary>
        User_Profile_Url = 21,

        /// <summary>The URL was unfriendly for a child portal.</summary>
        Unfriendly_Url_Child_Portal = 22,

        /// <summary>The URL contained a tab ID.</summary>
        Unfriendly_Url_TabId = 23,

        /// <summary>There was an error.</summary>
        Error_Event = 24,

        /// <summary>There was not a matching portal alias.</summary>
        No_Portal_Alias = 25,

        /// <summary>The request was for a 404 response.</summary>
        Requested_404 = 26,

        /// <summary>The URL requested a 404 response.</summary>
        Requested_404_In_Url = 27,

        /// <summary>The page is a 404.</summary>
        Page_404 = 28,

        /// <summary>There was an exception.</summary>
        Exception = 29,

        /// <summary>The URL was for a page that links to a file.</summary>
        File_Url = 30,

        /// <summary>The URL was a for a built-in path (e.g. terms, privacy, login, or register).</summary>
        Built_In_Url = 31,

        /// <summary>The URL matched a entry in the SiteUrls config.</summary>
        SiteUrls_Config_Rule = 32,

        /// <summary>The URL contained diacritic characters.</summary>
        Diacritic_Characters = 33,

        /// <summary>A module provider rewrite indicated there was a redirect.</summary>
        Module_Provider_Rewrite_Redirect = 34,

        /// <summary>A module provider indicated there was a redirect.</summary>
        Module_Provider_Redirect = 35,

        /// <summary>The splash page was requested.</summary>
        Requested_SplashPage = 36,

        /// <summary>A non-permanent redirect.</summary>
        Tab_Temporary_Redirect = 37,
    }
#pragma warning restore CA1707
}
