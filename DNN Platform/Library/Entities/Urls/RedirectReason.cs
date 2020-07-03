// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    public enum RedirectReason
    {
        Unfriendly_Url_1,
        Unfriendly_Url_2,
        Not_Lower_Case,
        Wrong_Sub_Domain,
        Custom_Redirect,
        Wrong_Portal,
        Not_Redirected,
        Deleted_Page,
        Disabled_Page,
        Spaces_Replaced,
        Wrong_Portal_Alias,
        Wrong_Portal_Alias_For_Browser_Type,
        Wrong_Portal_Alias_For_Culture,
        Wrong_Portal_Alias_For_Culture_And_Browser,
        Custom_Tab_Alias,
        Site_Root_Home,
        Secure_Page_Requested,
        Tab_External_Url,
        Tab_Permanent_Redirect,
        Host_Portal_Used,
        Alias_In_Url,
        User_Profile_Url,
        Unfriendly_Url_Child_Portal,
        Unfriendly_Url_TabId,
        Error_Event,
        No_Portal_Alias,
        Requested_404,
        Requested_404_In_Url,
        Page_404,
        Exception,
        File_Url,
        Built_In_Url,
        SiteUrls_Config_Rule,
        Diacritic_Characters,
        Module_Provider_Rewrite_Redirect,
        Module_Provider_Redirect,
        Requested_SplashPage,
    }
}
