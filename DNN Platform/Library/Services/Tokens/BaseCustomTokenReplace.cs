// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            if (this.PropertySource.ContainsKey(objectName.ToLowerInvariant()))
            {
                result = this.PropertySource[objectName.ToLowerInvariant()].GetProperty(propertyName, format, this.FormatProvider, this.AccessingUser, this.CurrentAccessLevel, ref propertyNotFound);
            }
            else
            {
                if (this.DebugMessages)
                {
                    string message = Localization.Localization.GetString("TokenReplaceUnknownObject", Localization.Localization.SharedResourceFile, this.FormatProvider.ToString());
                    if (message == string.Empty)
                    {
                        message = "Error accessing [{0}:{1}], {0} is an unknown datasource";
                    }

                    result = string.Format(message, objectName, propertyName);
                }
            }

            if (this.DebugMessages && propertyNotFound)
            {
                string message;
                if (result == PropertyAccess.ContentLocked)
                {
                    message = Localization.Localization.GetString("TokenReplaceRestrictedProperty", Localization.Localization.GlobalResourceFile, this.FormatProvider.ToString());
                }
                else
                {
                    message = Localization.Localization.GetString("TokenReplaceUnknownProperty", Localization.Localization.GlobalResourceFile, this.FormatProvider.ToString());
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
                // initialize PropertyAccess classes
                string DummyResult = this.ReplaceTokens(sourceText);

                foreach (Match currentMatch in this.TokenizerRegex.Matches(sourceText))
                {
                    string strObjectName = currentMatch.Result("${object}");
                    if (!string.IsNullOrEmpty(strObjectName))
                    {
                        if (strObjectName == "[")
                        {
                            // nothing
                        }
                        else if (!this.PropertySource.ContainsKey(strObjectName.ToLowerInvariant()))
                        {
                        }
                        else
                        {
                            CacheLevel c = this.PropertySource[strObjectName.ToLowerInvariant()].Cacheability;
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
                return this.TokenizerRegex.Matches(strSourceText).Cast<Match>().Any(currentMatch => currentMatch.Result("${object}").Length > 0);
            }

            return false;
        }
    }
}
