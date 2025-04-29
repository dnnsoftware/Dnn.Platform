// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

public enum RedirectReason
{
    Unfriendly_Url_1 = 0,
    Unfriendly_Url_2 = 1,
    Not_Lower_Case = 2,
    Wrong_Sub_Domain = 3,
    Custom_Redirect = 4,
    Wrong_Portal = 5,
    Not_Redirected = 6,
    Deleted_Page = 7,
    Disabled_Page = 8,
    Spaces_Replaced = 9,
    Wrong_Portal_Alias = 10,
    Wrong_Portal_Alias_For_Browser_Type = 11,
    Wrong_Portal_Alias_For_Culture = 12,
    Wrong_Portal_Alias_For_Culture_And_Browser = 13,
    Custom_Tab_Alias = 14,
    Site_Root_Home = 15,
    Secure_Page_Requested = 16,
    Tab_External_Url = 17,
    Tab_Permanent_Redirect = 18,
    Host_Portal_Used = 19,
    Alias_In_Url = 20,
    User_Profile_Url = 21,
    Unfriendly_Url_Child_Portal = 22,
    Unfriendly_Url_TabId = 23,
    Error_Event = 24,
    No_Portal_Alias = 25,
    Requested_404 = 26,
    Requested_404_In_Url = 27,
    Page_404 = 28,
    Exception = 29,
    File_Url = 30,
    Built_In_Url = 31,
    SiteUrls_Config_Rule = 32,
    Diacritic_Characters = 33,
    Module_Provider_Rewrite_Redirect = 34,
    Module_Provider_Redirect = 35,
    Requested_SplashPage = 36,
    Tab_Temporary_Redirect = 37,
}
