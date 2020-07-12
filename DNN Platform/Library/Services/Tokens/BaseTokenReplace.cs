// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    using DotNetNuke.Common.Utilities;

    /// <summary>
    /// The BaseTokenReplace class provides the tokenization of tokens formatted  
    /// [object:property] or [object:property|format|ifEmpty] or [custom:no] within a string
    /// with the appropriate current property/custom values.
    /// </summary>
    /// <remarks></remarks>
    public abstract class BaseTokenReplace
    {

        protected const string ObjectLessToken = "no_object";
        private const string ExpressionDefault =
            "(?:(?<text>\\[\\])|\\[(?:(?<object>[^{}\\]\\[:]+):(?<property>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])|(?<text>\\[[^\\]\\[]+\\])|(?<text>[^\\]\\[]+)";

        private const string ExpressionObjectLess =
            "(?:(?<text>\\[\\])|\\[(?:(?<object>[^{}\\]\\[:]+):(?<property>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])" +
            "|(?:(?<object>\\[)(?<property>[A-Z0-9._]+)(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])" + "|(?<text>\\[[^\\]\\[]+\\])" +
            "|(?<text>[^\\]\\[]+)";

        private const string TokenReplaceCacheKeyDefault = "TokenReplaceRegEx_Default";
        private const string TokenReplaceCacheKeyObjectless = "TokenReplaceRegEx_Objectless";


        private CultureInfo _formatProvider;
        private string _language;

        /// <summary>
        /// Gets the Format provider as Culture info from stored language or current culture.
        /// </summary>
        /// <value>An CultureInfo.</value>
        protected virtual CultureInfo FormatProvider
        {
            get { return _formatProvider ?? (_formatProvider = Thread.CurrentThread.CurrentUICulture); }
        }

        /// <summary>
        /// Gets or sets /sets the language to be used, e.g. for date format.
        /// </summary>
        /// <value>A string, representing the locale.</value>
        public virtual string Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
                _formatProvider = new CultureInfo(_language);
            }
        }

        /// <summary>
        /// Gets the Regular expression for the token to be replaced.
        /// </summary>
        /// <value>A regular Expression.</value>
        protected Regex TokenizerRegex
        {
            get
            {
                var cacheKey = this.UseObjectLessExpression ? TokenReplaceCacheKeyObjectless : TokenReplaceCacheKeyDefault;
                var tokenizer = DataCache.GetCache(cacheKey) as Regex;
                if (tokenizer == null)
                {
                    tokenizer = RegexUtils.GetCachedRegex(this.UseObjectLessExpression ? ExpressionObjectLess : ExpressionDefault);
                    DataCache.SetCache(cacheKey, tokenizer);
                }

                return tokenizer;
            }
        }

        protected bool UseObjectLessExpression { get; set; }

        // ReSharper disable once InconsistentNaming
        protected abstract string replacedTokenValue(string objectName, string propertyName, string format);

        protected virtual string ReplaceTokens(string sourceText)
        {
            if (sourceText == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            foreach (Match currentMatch in this.TokenizerRegex.Matches(sourceText))
            {
                string objectName = currentMatch.Result("${object}");
                if (!string.IsNullOrEmpty(objectName))
                {
                    if (objectName == "[")
                    {
                        objectName = ObjectLessToken;
                    }

                    string propertyName = currentMatch.Result("${property}");
                    string format = currentMatch.Result("${format}");
                    string ifEmptyReplacment = currentMatch.Result("${ifEmpty}");
                    string conversion = this.replacedTokenValue(objectName, propertyName, format);
                    if (!string.IsNullOrEmpty(ifEmptyReplacment) && string.IsNullOrEmpty(conversion))
                    {
                        conversion = ifEmptyReplacment;
                    }

                    result.Append(conversion);
                }
                else
                {
                    result.Append(currentMatch.Result("${text}"));
                }
            }

            return result.ToString();
        }
    }
}
