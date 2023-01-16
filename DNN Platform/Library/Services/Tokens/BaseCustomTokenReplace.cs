﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Users;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>BaseCustomTokenReplace allows to add multiple sources implementing <see cref="IPropertyAccess">IPropertyAccess</see>.</summary>
    public abstract class BaseCustomTokenReplace : BaseTokenReplace
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected Dictionary<string, IPropertyAccess> PropertySource;

        private TokenContext tokenContext = new TokenContext();

        /// <summary>Initializes a new instance of the <see cref="BaseCustomTokenReplace"/> class.</summary>
        public BaseCustomTokenReplace()
        {
            this.PropertySource = this.TokenContext.PropertySource;
        }

        public TokenContext TokenContext
        {
            get => this.tokenContext;
            set
            {
                this.tokenContext = value;
                this.PropertySource = this.tokenContext.PropertySource;
            }
        }

        /// <summary>Gets or sets the user object representing the currently accessing user (permission).</summary>
        /// <value>UserInfo object.</value>
        public UserInfo AccessingUser
        {
            get => this.TokenContext.AccessingUser;
            set => this.TokenContext.AccessingUser = value;
        }

        /// <summary>Gets or sets a value indicating whether if DebugMessages are enabled, unknown Tokens are replaced with Error Messages.</summary>
        /// <remarks>If DebugMessages are enabled, unknown Tokens are replaced with Error Messages.</remarks>
        public bool DebugMessages
        {
            get => this.TokenContext.DebugMessages;
            set => this.TokenContext.DebugMessages = value;
        }

        /// <summary>Gets or sets the language to be used, e.g. for date format.</summary>
        /// <value>A string, representing the locale.</value>
        public override string Language
        {
            get => this.TokenContext.Language.ToString();
            set
            {
                this.TokenContext.Language = new CultureInfo(value);
            }
        }

        protected TokenProvider Provider
        {
            get => ComponentFactory.GetComponent<TokenProvider>();
        }

        /// <summary>Gets the Format provider as Culture info from stored language or current culture.</summary>
        protected override CultureInfo FormatProvider
        {
            get => this.TokenContext.Language;
        }

        /// <summary>Gets or sets the current Access Level controlling access to critical user settings.</summary>
        protected Scope CurrentAccessLevel
        {
            get => this.TokenContext.CurrentAccessLevel;
            set => this.TokenContext.CurrentAccessLevel = value;
        }

        /// <summary>returns cacheability of the passed text regarding all contained tokens.</summary>
        /// <param name="sourceText">the text to parse for tokens to replace.</param>
        /// <returns>cacheability level (not - safe - fully).</returns>
        /// <remarks>always check cacheability before caching a module!.</remarks>
        public CacheLevel Cacheability(string sourceText)
        {
            CacheLevel isSafe = CacheLevel.fullyCacheable;
            if (sourceText != null && !string.IsNullOrEmpty(sourceText))
            {
                // initialize PropertyAccess classes
                string dummyResult = this.ReplaceTokens(sourceText);

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

        /// <summary>Checks for present <c>[Object:Property]</c> tokens.</summary>
        /// <param name="strSourceText">String with <c>[Object:Property]</c> tokens.</param>
        /// <returns><see langword="true"/> if the source text contains tokens, otherwise <see langword="false"/>.</returns>
        public bool ContainsTokens(string strSourceText)
        {
            if (string.IsNullOrEmpty(strSourceText))
            {
                return false;
            }

            // also check providers, since they might support different syntax than square brackets
            return this.TokenizerRegex.Matches(strSourceText).Cast<Match>().Any(currentMatch => currentMatch.Result("${object}").Length > 0)
                || this.Provider.ContainsTokens(strSourceText, this.TokenContext);
        }

        /// <inheritdoc/>
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
                    string message = Localization.GetString("TokenReplaceUnknownObject", Localization.SharedResourceFile, this.FormatProvider.ToString());
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
                    message = Localization.GetString("TokenReplaceRestrictedProperty", Localization.GlobalResourceFile, this.FormatProvider.ToString());
                }
                else
                {
                    message = Localization.GetString("TokenReplaceUnknownProperty", Localization.GlobalResourceFile, this.FormatProvider.ToString());
                }

                if (message == string.Empty)
                {
                    message = "Error accessing [{0}:{1}], {1} is unknown for datasource {0}";
                }

                result = string.Format(message, objectName, propertyName);
            }

            return result;
        }

        /// <inheritdoc/>
        protected override string ReplaceTokens(string sourceText)
        {
            return this.Provider is CoreTokenProvider ? base.ReplaceTokens(sourceText) : this.Provider.Tokenize(sourceText, this.TokenContext);
        }
    }
}
