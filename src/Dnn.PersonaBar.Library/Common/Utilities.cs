#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dnn.PersonaBar.Library.Common
{
    public class Utilities
    {
        private static readonly DateTime MinSocialTime = new DateTime(2000, 1, 1);

        #region Private members

        private static readonly Regex HtmlRegex = new Regex("<[^<>]*>", RegexOptions.Compiled, TimeSpan.FromSeconds(2));

        #endregion

        #region Text transformation methods

        /// <summary>
        /// Take a numerator and a denominator and create a human-readable percentage.
        /// </summary>
        public static string CalucalatePercentForDisplay(int numerator, int denominator)
        {
            if (denominator != 0)
            {
                if ((int)(((double)numerator / denominator) * 100) >= 100)
                {
                    return @"100%";
                }

                return (int)(((double)numerator / denominator) * 100) + @"%";
            }

            return "0%";
        }

        /// <summary>
        /// Shorten a raw number to a more friendly human display
        /// </summary>
        public static string CalculateCountForDisplay(long count)
        {
            if (count < 0)
            {
                return count.ToString(CultureInfo.InvariantCulture);
            }

            const long kilobyte = 1024;
            const long megabyte = kilobyte * 1024;
            const long gigabyte = megabyte * 1024;
            const long terabyte = gigabyte * 1024;
            const long petabyte = terabyte * 1024;

            if (count >= petabyte)
                return string.Format("{0:0}P", count / petabyte);
            if (count >= terabyte)
                return string.Format("{0:0}T", count / terabyte);
            if (count >= gigabyte)
                return string.Format("{0:0}G", count / gigabyte);
            if (count >= megabyte)
                return string.Format("{0:0}M", count / megabyte);
            if (count >= kilobyte)
                return string.Format("{0:0}K", count / kilobyte);

            return string.Format("{0:0}", count);
        }

        public static string CalculateCountForDisplay(int count)
        {
            return CalculateCountForDisplay((long)count);
        }

        // test sample
        //"&agrave;&aacute;&acirc;&atilde;&auml;&aring;&aelig;&ccedil;&egrave;&eacute;&ecirc;&euml;&igrave;&iacute;&icirc;&iuml;&Alpha;
        //&ETH;&ntilde;&ograve;&oacute;&ocirc;&otilde;&ouml;&oslash;&ugrave;&uacute;&ucirc;&uuml;&yacute;&yuml;&szlig;&thorn;&omicron;"
        private const string HtmlEntitiesPattern = @"&amp;([a-z]{2,10}|#\d{1,10}|#x[0-9a-f]{1,8});";
        private static readonly Regex HtmlEntitiesPatternRegex =
            new Regex(HtmlEntitiesPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));

        public static string FixDoublEntityEncoding(string document)
        {
            return string.IsNullOrEmpty(document)
                       ? document
                       : HtmlEntitiesPatternRegex.Replace(document, "&$1;");
        }

        #endregion

        /// <summary>
        /// Does the specified input string <paramref name="input"/> contain disallowed characters?
        /// </summary>
        public static bool ContainsSpecialCharacter(string input)
        {
            // Characters that cause no results.
            //      \ - Backslash
            //      ; - Semi-colon
            //      ' - single quote
            // Characters that cause dangerous page request validation errors.
            //      &?*%@

            return input.IndexOfAny(Constants.DisallowedCharacters.ToCharArray()) != -1;
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// </summary>
        public static string StripTagsCharArray(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var array = new char[source.Length];
            var arrayIndex = 0;
            var inside = false;

            foreach (var @let in source)
            {
                if (@let == '<')
                {
                    inside = true;
                }
                else if (@let == '>')
                {
                    inside = false;
                }
                else if (inside == false)
                {
                    array[arrayIndex++] = @let;
                }
            }

            return new string(array, 0, arrayIndex);
        }

        /// <summary>
        /// Trims a string to supplied maxLength and replaces the last three characters with three dots
        /// </summary>
        public static string TrimWithEllipsis(string content, int maxLength = 50)
        {
            content = content.Trim();
            return !string.IsNullOrEmpty(content) && maxLength > 3 && content.Length > maxLength
                       ? content.Substring(0, maxLength - 3) + "..."
                       : content;
        }

        public static string RemoveHtmlTags(string content)
        {
            return string.IsNullOrEmpty(content) ? string.Empty : HtmlRegex.Replace(content, string.Empty);
        }

        public static string RemoveHtmlTagsWithSpace(string content)
        {
            return HtmlRegex.Replace(content, " ").Replace("  ", " ").Trim();
        }

        public static T GetSettingValue<T>(Hashtable settings, string settingName, T defaultValue)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException("settingName");
            }

            if (!settings.ContainsKey(settingName) || settings[settingName] == null)
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(settings[settingName], typeof(T));
        }

        public static string SerializeObjectToJson(object obj)
        {
            var dtc = new IsoDateTimeConverter();
            dtc.DateTimeFormat = "yyyy-MM-dd";
            return JsonConvert.SerializeObject(obj, dtc);            
        }

        public static string RelativeDateFromUtcDate(DateTime utcDdate)
        {
            var dbTime = DateUtils.GetDatabaseTime();

            if (dbTime < MinSocialTime) dbTime = MinSocialTime.AddYears(-10);
            if (utcDdate < MinSocialTime) utcDdate = MinSocialTime;

            var difference = dbTime - utcDdate;

            if (difference.TotalSeconds < 60)
            {
                return Localization.GetString("JustNow", Constants.SharedResources);
            }

            if (difference.TotalMinutes < 60)
            {
                var minutes = Convert.ToInt32(difference.TotalMinutes);
                return string.Format(
                    Localization.GetString(minutes == 1 ? "MinuteAgo" : "MinutesAgo", Constants.SharedResources), minutes);
            }

            if (difference.TotalHours < 24)
            {
                var hours = Convert.ToInt32(difference.TotalHours);
                return string.Format(
                    Localization.GetString(hours == 1 ? "HourAgo" : "HoursAgo", Constants.SharedResources), hours);
            }

            if (difference.TotalDays < 7)
            {
                var days = Convert.ToInt32(difference.TotalDays);
                return string.Format(
                    Localization.GetString(days == 1 ? "DayAgo" : "DaysAgo", Constants.SharedResources), days);
            }

            if (difference.TotalDays < 30)
            {
                var weeks = Convert.ToInt32(Math.Floor(difference.TotalDays / 7));
                return string.Format(
                    Localization.GetString(weeks == 1 ? "WeekAgo" : "WeeksAgo", Constants.SharedResources), weeks);
            }

            if (difference.TotalDays <= 365)
            {
                var months = Convert.ToInt32(Math.Floor(difference.TotalDays / 30));
                return string.Format(
                    Localization.GetString(months == 1 ? "MonthAgo" : "MonthsAgo", Constants.SharedResources), months);
            }

            if (difference.TotalDays <= (365 * 10))
            {
                var years = Convert.ToInt32(Math.Floor(difference.TotalDays / 365));
                return string.Format(
                    Localization.GetString(years == 1 ? "YearAgo" : "YearsAgo", Constants.SharedResources), years);
            }

            return Localization.GetString("LongTimeAgo", Constants.SharedResources);
        }

        /// <summary>
        /// Finds a user in Portal or Non-Portal (Host)
        /// </summary>
        public static UserInfo GetUserById(int portalId, int userId)
        {
            return UserController.GetUserById(portalId, userId) ??
                   UserController.GetUserById(Null.NullInteger, userId);
        }

        /// <summary>
        /// Get User's standard Profile avatar. The Url is resolved to current portal
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <returns>user profile picture url</returns>
        public static string GetProfileAvatar(int userId)
        {
            var url = UserController.Instance.GetUserProfilePictureUrl(userId, Constants.AvatarWidth, Constants.AvatarHeight);
            return Globals.ResolveUrl(url);
        }

        /// <summary>
        /// Returns a relative URL for the user profile image while removing that of the deleted and super users
        /// </summary>
        /// <param name="user">user info</param>
        /// <param name="width">width in pixel</param>
        /// <param name="height">height in pixel</param>
        /// <param name="showSuperUsers">true if want show super users user profile picture, false otherwise</param>
        /// <returns>relative user profile picture url</returns>
        /// <returns></returns>
        public static string GetProfileAvatar(UserInfo user, int width = Constants.AvatarWidth, int height = Constants.AvatarHeight, bool showSuperUsers = true)
        {
            var userId = user != null && user.UserID > 0 && !user.IsDeleted && (showSuperUsers || !user.IsSuperUser) ? user.UserID : 0;
            return UserController.Instance.GetUserProfilePictureUrl(userId, width, height);
        }

        /// <summary>
        /// Returns a relative URL for the user profile image while removing that of the deleted
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <param name="portalId">portal Id</param>
        /// <param name="width">width in pixel</param>
        /// <param name="height">height in pixel</param>
        /// <param name="showSuperUsers">true if want show super users user profile picture, false otherwise</param>
        /// <returns>relative user profile picture url</returns>
        public static string GetProfileAvatar(int userId, int portalId, int width = Constants.AvatarWidth, int height = Constants.AvatarHeight, bool showSuperUsers = true)
        {
            var user = userId > 0 ? UserController.GetUserById(portalId, userId) : null;
            return GetProfileAvatar(user, width, height, showSuperUsers);
        }

        /// <summary>
        /// Returns a absolute URL for the user profile image while removing that of the deleted and super users
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="user">user info</param>
        /// <param name="width">width in pixel</param>
        /// <param name="height">height in pixel</param>
        /// <param name="showSuperUsers">true if want show super users user profile picture, false otherwise</param>
        /// <returns>absolute user profile picture url</returns>
        /// <returns></returns>
        public static string GetProfileAvatarAbsoluteUrl(int portalId, UserInfo user, int width = Constants.AvatarWidth,
            int height = Constants.AvatarHeight, bool showSuperUsers = true)
        {
            var userId = user != null && user.UserID > 0 && !user.IsDeleted && (showSuperUsers || !user.IsSuperUser) ? user.UserID : 0;
            var relativePath = GetUserProfilePictureUrl(portalId, userId, width, height);
            return GetAbsoluteUrl(portalId, relativePath);
        }

        /// <summary>
        /// Return User Profile Picture relative Url
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="width">Width in pixel</param>
        /// <param name="height">Height in pixel</param>
        /// <returns>Relative url,  e.g. /DnnImageHandler.ashx?userid=1&amp;h=32&amp;w=32 considering child portal</returns>
        /// <remarks>IMPORTANT NOTE: this has been copied from the Platform IUserController.GetUserProfilePictureUrl
        /// The method in platform depends on the current portal setting so cannot be used in background thread or scheduler jobs. 
        /// To fit requirements for Evoq 8.5.0 we have copied and modified this method here to support a portalId input parameter. 
        /// Unfortunately we had no way to create this method overload in Platform as at that point it was frozed for 8.0.4.
        /// This method will be moved to Platform: see jira CONTENT-6674
        /// </remarks>
        private static string GetUserProfilePictureUrl(int portalId, int userId, int width, int height)
        {
            var url = $"/DnnImageHandler.ashx?mode=profilepic&userId={userId}&h={width}&w={height}";

            var childPortalAlias = GetChildPortalAlias(portalId);
            var cdv = GetProfilePictureCdv(portalId, userId);

            return childPortalAlias.StartsWith(Globals.ApplicationPath)
                ? childPortalAlias + url + cdv
                : Globals.ApplicationPath + childPortalAlias + url + cdv;
        }

        private static string GetChildPortalAlias(int portalId)
        {
            var portalAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId)
                                .OrderByDescending(a => a.IsPrimary)
                                .First();
            var currentAlias = portalAlias.HTTPAlias;
            var index = currentAlias.IndexOf('/');
            var childPortalAlias = index > 0 ? "/" + currentAlias.Substring(index + 1) : "";
            return childPortalAlias;
        }

        private static string GetProfilePictureCdv(int portalId, int userId)
        {
            var userInfo = GetUserById(portalId, userId);
            if (userInfo?.Profile == null)
            {
                return string.Empty;
            }

            var cdv = string.Empty;
            var photoProperty = userInfo.Profile.GetProperty("Photo");

            int photoFileId;
            if (int.TryParse(photoProperty?.PropertyValue, out photoFileId))
            {
                var photoFile = FileManager.Instance.GetFile(photoFileId);
                if (photoFile != null)
                {
                    cdv = "&cdv=" + photoFile.LastModifiedOnDate.Ticks;
                }
            }
            return cdv;
        }

        /// <summary>
        /// Returns an absolute url given a relative url
        /// </summary>
        /// <param name="portalId">portal Id</param>
        /// <param name="relativeUrl">relative url</param>
        /// <returns>absolute url</returns>
        public static string GetAbsoluteUrl(int portalId, string relativeUrl)
        {
            if (relativeUrl.Contains("://"))
            {
                return relativeUrl;
            }
            var portalAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).First(p => p.IsPrimary);
            var domainName = GetDomainName(portalAlias);
            return Globals.AddHTTP(domainName + relativeUrl);
        }

        private static string GetDomainName(PortalAliasInfo portalAlias)
        {
            var httpAlias = portalAlias.HTTPAlias;
            return httpAlias.IndexOf("/", StringComparison.InvariantCulture) != -1 ?
                        httpAlias.Substring(0, httpAlias.IndexOf("/", StringComparison.InvariantCulture)) :
                        httpAlias;
        }
    }
}
