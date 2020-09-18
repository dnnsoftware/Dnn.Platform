// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Common
{
    using System;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;

    public class Utilities
    {
        // test sample
        // "&agrave;&aacute;&acirc;&atilde;&auml;&aring;&aelig;&ccedil;&egrave;&eacute;&ecirc;&euml;&igrave;&iacute;&icirc;&iuml;&Alpha;
        // &ETH;&ntilde;&ograve;&oacute;&ocirc;&otilde;&ouml;&oslash;&ugrave;&uacute;&ucirc;&uuml;&yacute;&yuml;&szlig;&thorn;&omicron;"
        private const string HtmlEntitiesPattern = @"&amp;([a-z]{2,10}|#\d{1,10}|#x[0-9a-f]{1,8});";

        private static readonly Regex HtmlEntitiesPatternRegex =
            new Regex(HtmlEntitiesPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));

        /// <summary>
        /// Returns a relative URL for the user profile image while removing that of the deleted and super users.
        /// </summary>
        /// <param name="user">user info.</param>
        /// <param name="width">width in pixel.</param>
        /// <param name="height">height in pixel.</param>
        /// <param name="showSuperUsers">true if want show super users user profile picture, false otherwise.</param>
        /// <returns>relative user profile picture url.</returns>
        /// <returns></returns>
        public static string GetProfileAvatar(UserInfo user, int width = Constants.AvatarWidth, int height = Constants.AvatarHeight, bool showSuperUsers = true)
        {
            var userId = user != null && user.UserID > 0 && !user.IsDeleted && (showSuperUsers || !user.IsSuperUser) ? user.UserID : 0;
            return UserController.Instance.GetUserProfilePictureUrl(userId, width, height);
        }

        /// <summary>
        /// Get User's standard Profile avatar. The Url is resolved to current portal.
        /// </summary>
        /// <param name="userId">user Id.</param>
        /// <returns>user profile picture url.</returns>
        public static string GetProfileAvatar(int userId)
        {
            var url = UserController.Instance.GetUserProfilePictureUrl(userId, Constants.AvatarWidth, Constants.AvatarHeight);
            return Globals.ResolveUrl(url);
        }

        public static string FixDoublEntityEncoding(string document)
        {
            return string.IsNullOrEmpty(document)
                       ? document
                       : HtmlEntitiesPatternRegex.Replace(document, "&$1;");
        }
    }
}
