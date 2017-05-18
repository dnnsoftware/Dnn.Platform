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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// BaseCustomTokenReplace  allows to add multiple sources implementing <see cref="IPropertyAccess">IPropertyAccess</see>
    /// </summary>
    /// <remarks></remarks>
    public abstract class BaseCustomTokenReplace : BaseTokenReplace
    {
        protected Dictionary<string, IPropertyAccess> PropertySource = new Dictionary<string, IPropertyAccess>();

        /// <summary>
        /// Gets/sets the user object representing the currently accessing user (permission)
        /// </summary>
        /// <value>UserInfo oject</value>
        public UserInfo AccessingUser { get; set; }

        /// <summary>
        /// Gets/sets the current Access Level controlling access to critical user settings
        /// </summary>
        /// <value>A TokenAccessLevel as defined above</value>
        protected Scope CurrentAccessLevel { get; set; }

        /// <summary>
        /// If DebugMessages are enabled, unknown Tokens are replaced with Error Messages 
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool DebugMessages { get; set; }

        protected override string replacedTokenValue(string objectName, string propertyName, string format)
        {
            string result = string.Empty;
            bool propertyNotFound = false;
            if (PropertySource.ContainsKey(objectName.ToLower()))
            {
                result = PropertySource[objectName.ToLower()].GetProperty(propertyName, format, FormatProvider, AccessingUser, CurrentAccessLevel, ref propertyNotFound);
            }
            else
            {
                if (DebugMessages)
                {
                    string message = Localization.Localization.GetString("TokenReplaceUnknownObject", Localization.Localization.SharedResourceFile, FormatProvider.ToString());
                    if (message == string.Empty)
                    {
                        message = "Error accessing [{0}:{1}], {0} is an unknown datasource";
                    }
                    result = string.Format(message, objectName, propertyName);
                }
            }
            if (DebugMessages && propertyNotFound)
            {
                string message;
                if (result == PropertyAccess.ContentLocked)
                {
                    message = Localization.Localization.GetString("TokenReplaceRestrictedProperty", Localization.Localization.GlobalResourceFile, FormatProvider.ToString());
                }
                else
                {
                    message = Localization.Localization.GetString("TokenReplaceUnknownProperty", Localization.Localization.GlobalResourceFile, FormatProvider.ToString());
                }
                if (message == string.Empty)
                {
                    message = "Error accessing [{0}:{1}], {1} is unknown for datasource {0}";
                }
                result = string.Format(message, objectName, propertyName);
            }

            return result;
        }

        /// <summary>
        /// returns cacheability of the passed text regarding all contained tokens
        /// </summary>
        /// <param name="sourceText">the text to parse for tokens to replace</param>
        /// <returns>cacheability level (not - safe - fully)</returns>
        /// <remarks>always check cacheability before caching a module!</remarks>
        public CacheLevel Cacheability(string sourceText)
        {
            CacheLevel isSafe = CacheLevel.fullyCacheable;
            if (sourceText != null && !string.IsNullOrEmpty(sourceText))
            {
                //initialize PropertyAccess classes
                string DummyResult = ReplaceTokens(sourceText);

                foreach (Match currentMatch in TokenizerRegex.Matches(sourceText))
                {
                    string strObjectName = currentMatch.Result("${object}");
                    if (!String.IsNullOrEmpty(strObjectName))
                    {
                        if (strObjectName == "[")
                        {
                            //nothing
                        }
                        else if (!PropertySource.ContainsKey(strObjectName.ToLower()))
                        {
                        }
                        else
                        {
                            CacheLevel c = PropertySource[strObjectName.ToLower()].Cacheability;
                            if (c < isSafe)
                            {
                                isSafe = c;
                            }
                        }
                    }
                }
            }
            return isSafe;
        }

        /// <summary>
        /// Checks for present [Object:Property] tokens
        /// </summary>
        /// <param name="strSourceText">String with [Object:Property] tokens</param>
        /// <returns></returns>
        public bool ContainsTokens(string strSourceText)
        {
            if (!string.IsNullOrEmpty(strSourceText))
            {
                return TokenizerRegex.Matches(strSourceText).Cast<Match>().Any(currentMatch => currentMatch.Result("${object}").Length > 0);
            }
            return false;
        }
    }
}
