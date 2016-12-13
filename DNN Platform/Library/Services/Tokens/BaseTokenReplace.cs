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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// The BaseTokenReplace class provides the tokenization of tokens formatted  
    /// [object:property] or [object:property|format|ifEmpty] or [custom:no] within a string
    /// with the appropriate current property/custom values.
    /// </summary>
    /// <remarks></remarks>
    public abstract class BaseTokenReplace
    {
        private const string ExpressionDefault =
            "(?:(?<text>\\[\\])|\\[(?:(?<object>[^\\]\\[:]+):(?<property>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])|(?<text>\\[[^\\]\\[]+\\])|(?<text>[^\\]\\[]+)";

        private const string ExpressionObjectLess =
            "(?:(?<text>\\[\\])|\\[(?:(?<object>[^\\]\\[:]+):(?<property>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])" +
            "|(?:(?<object>\\[)(?<property>[A-Z0-9._]+)(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])" + "|(?<text>\\[[^\\]\\[]+\\])" +
            "|(?<text>[^\\]\\[]+)";

        private const string TokenReplaceCacheKeyDefault = "TokenReplaceRegEx_Default";
        private const string TokenReplaceCacheKeyObjectless = "TokenReplaceRegEx_Objectless";


        private CultureInfo _formatProvider;
        private string _language;

        protected const string ObjectLessToken = "no_object";

        protected bool UseObjectLessExpression { get; set; }

        /// <summary>
        /// Gets the Format provider as Culture info from stored language or current culture
        /// </summary>
        /// <value>An CultureInfo</value>
        protected CultureInfo FormatProvider
        {
            get { return _formatProvider ?? (_formatProvider = Thread.CurrentThread.CurrentUICulture); }
        }

        /// <summary>
        /// Gets/sets the language to be used, e.g. for date format
        /// </summary>
        /// <value>A string, representing the locale</value>
        public string Language
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
        /// Gets the Regular expression for the token to be replaced
        /// </summary>
        /// <value>A regular Expression</value>   
        protected Regex TokenizerRegex
        {
            get
            {
                var cacheKey = (UseObjectLessExpression) ? TokenReplaceCacheKeyObjectless : TokenReplaceCacheKeyDefault;
                var tokenizer = DataCache.GetCache(cacheKey) as Regex;
                if (tokenizer == null)
                {
                    tokenizer = RegexUtils.GetCachedRegex(UseObjectLessExpression ? ExpressionObjectLess : ExpressionDefault);
                    DataCache.SetCache(cacheKey, tokenizer);
                }
                return tokenizer;
            }
        }

        // ReSharper disable once InconsistentNaming
        protected abstract string replacedTokenValue(string objectName, string propertyName, string format);

        protected virtual string ReplaceTokens(string sourceText)
        {
            if (sourceText == null)
            {
                return string.Empty;
            }
            var result = new StringBuilder();
            foreach (Match currentMatch in TokenizerRegex.Matches(sourceText))
            {
                string objectName = currentMatch.Result("${object}");
                if (!String.IsNullOrEmpty(objectName))
                {
                    if (objectName == "[")
                    {
                        objectName = ObjectLessToken;
                    }
                    string propertyName = currentMatch.Result("${property}");
                    string format = currentMatch.Result("${format}");
                    string ifEmptyReplacment = currentMatch.Result("${ifEmpty}");
                    string conversion = replacedTokenValue(objectName, propertyName, format);
                    if (!String.IsNullOrEmpty(ifEmptyReplacment) && String.IsNullOrEmpty(conversion))
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
