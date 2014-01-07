#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
#region Usings

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Services.Tokens
{
	#region " Type Definitions "

    /// <summary>
    /// Scope informs the property access classes about the planned usage of the token
    /// </summary>
    /// <remarks>
    /// The result of a token replace operation depends on the current context, privacy settings
    /// and the current scope. The scope should be the lowest scope needed for the current purpose.
    /// The property access classes should evaluate and use the scope before returning a value.
    /// </remarks>
    public enum Scope
    {
		/// <summary>
		/// Only access to Date and Time
		/// </summary>
        NoSettings = 0,
		/// <summary>
		/// Tokens for Host, Portal, Tab (, Module), user name
		/// </summary>
        Configuration = 1,
		/// <summary>
		/// Configuration, Current User data and user data allowed for registered members
		/// </summary>
        DefaultSettings = 2,
		/// <summary>
		/// System notifications to users and adminstrators
		/// </summary>
        SystemMessages = 3,
		/// <summary>
		/// internal debugging, error messages, logs
		/// </summary>
        Debug = 4
    }

    /// <summary>
    /// CacheLevel is used to specify the cachability of a string, determined as minimum of the used token cachability
    /// </summary>
    /// <remarks>
    /// CacheLevel is determined as minimum of the used tokens' cachability 
    /// </remarks>
    public enum CacheLevel : byte
    {
        /// <summary>
        /// Caching of the text is not suitable and might expose security risks
        /// </summary>
        notCacheable = 0,
        /// <summary>
        /// Caching of the text might result in inaccurate display (e.g. time), but does not expose a security risk
        /// </summary>
        secureforCaching = 5,
        /// <summary>
        /// Caching of the text can be done without limitations or any risk
        /// </summary>
        fullyCacheable = 10
    }
	
	#endregion

    /// <summary>
    /// BaseCustomTokenReplace  allows to add multiple sources implementing <see cref="IPropertyAccess">IPropertyAccess</see>
    /// </summary>
    /// <remarks></remarks>
    public abstract class BaseCustomTokenReplace : BaseTokenReplace
    {
        protected Dictionary<string, IPropertyAccess> PropertySource = new Dictionary<string, IPropertyAccess>();
		#region "Protected Properties"

        /// <summary>
        /// Gets/sets the current Access Level controlling access to critical user settings
        /// </summary>
        /// <value>A TokenAccessLevel as defined above</value>
        protected Scope CurrentAccessLevel { get; set; }


		#endregion

		#region "Public Properties"

        /// <summary>
        /// Gets/sets the user object representing the currently accessing user (permission)
        /// </summary>
        /// <value>UserInfo oject</value>
        public UserInfo AccessingUser { get; set; }

        /// <summary>
        /// If DebugMessages are enabled, unknown Tokens are replaced with Error Messages 
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool DebugMessages { get; set; }

		#endregion

		#region "Protected Methods"

        protected override string replacedTokenValue(string strObjectName, string strPropertyName, string strFormat)
        {
            string result = string.Empty;
            bool PropertyNotFound = false;
            if (PropertySource.ContainsKey(strObjectName.ToLower()))
            {
                result = PropertySource[strObjectName.ToLower()].GetProperty(strPropertyName, strFormat, FormatProvider, AccessingUser, CurrentAccessLevel, ref PropertyNotFound);
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
                    result = string.Format(message, strObjectName, strPropertyName);
                }
            }
            if (DebugMessages && PropertyNotFound)
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
                result = string.Format(message, strObjectName, strPropertyName);
            }

            return result;
        }
		
		#endregion

		#region "Public Methods"

        /// <summary>
        /// Checks for present [Object:Property] tokens
        /// </summary>
        /// <param name="strSourceText">String with [Object:Property] tokens</param>
        /// <returns></returns>
        /// <history>
        ///    08/10/2007 [sleupold] created
        ///    10/19/2007 [sleupold] corrected to ignore unchanged text returned (issue DNN-6526)
        /// </history>
        public bool ContainsTokens(string strSourceText)
        {
            if (!string.IsNullOrEmpty(strSourceText))
            {
                foreach (Match currentMatch in TokenizerRegex.Matches(strSourceText))
                {
                    if (currentMatch.Result("${object}").Length > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// returns cacheability of the passed text regarding all contained tokens
        /// </summary>
        /// <param name="strSourcetext">the text to parse for tokens to replace</param>
        /// <returns>cacheability level (not - safe - fully)</returns>
        /// <remarks>always check cacheability before caching a module!</remarks>
        /// <history>
        ///    10/19/2007 [sleupold] corrected to handle non-empty strings
        /// </history>
        public CacheLevel Cacheability(string strSourcetext)
        {
            CacheLevel IsSafe = CacheLevel.fullyCacheable;
            if (strSourcetext != null && !string.IsNullOrEmpty(strSourcetext))
            {
                //initialize PropertyAccess classes
                string DummyResult = ReplaceTokens(strSourcetext);

                var Result = new StringBuilder();
                foreach (Match currentMatch in TokenizerRegex.Matches(strSourcetext))
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
                            if (c < IsSafe)
                            {
                                IsSafe = c;
                            }
                        }
                    }
                }
            }
            return IsSafe;
        }
		
		#endregion
    }
}
