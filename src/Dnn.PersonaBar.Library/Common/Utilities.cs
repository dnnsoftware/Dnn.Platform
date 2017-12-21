#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Text.RegularExpressions;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Library.Common
{
    public class Utilities
    {
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
        /// Get User's standard Profile avatar. The Url is resolved to current portal
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <returns>user profile picture url</returns>
        public static string GetProfileAvatar(int userId)
        {
            var url = UserController.Instance.GetUserProfilePictureUrl(userId, Constants.AvatarWidth, Constants.AvatarHeight);
            return Globals.ResolveUrl(url);
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

    }
}
