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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using DotNetNuke.Common.Utilities;

#endregion

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
            "(?:\\[(?:(?<object>[^\\]\\[:]+):(?<property>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])|(?<text>\\[[^\\]\\[]+\\])|(?<text>[^\\]\\[]+)";

        private const string ExpressionObjectLess =
            "(?:\\[(?:(?<object>[^\\]\\[:]+):(?<property>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])" +
            "|(?:(?<object>\\[)(?<property>[A-Z0-9._]+)(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])" + "|(?<text>\\[[^\\]\\[]+\\])" +
            "|(?<text>[^\\]\\[]+)";

        protected const string ObjectLessToken = "no_object";
        private CultureInfo _FormatProvider;
        private string _Language;

        protected bool UseObjectLessExpression { get; set; }

        private string TokenReplaceCacheKey
        {
            get
            {
                if (UseObjectLessExpression)
                {
                    return "TokenReplaceRegEx_Objectless";
                }
                else
                {
                    return "TokenReplaceRegEx_Default";
                }
            }
        }

        private string RegExpression
        {
            get
            {
                if (UseObjectLessExpression)
                {
                    return ExpressionObjectLess;
                }
                else
                {
                    return ExpressionDefault;
                }
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
                var tokenizer = (Regex) DataCache.GetCache(TokenReplaceCacheKey);
                if (tokenizer == null)
                {
                    tokenizer = new Regex(RegExpression, RegexOptions.Compiled);
                    DataCache.SetCache(TokenReplaceCacheKey, tokenizer);
                }
                return tokenizer;
            }
        }

        /// <summary>
        /// Gets/sets the language to be used, e.g. for date format
        /// </summary>
        /// <value>A string, representing the locale</value>
        public string Language
        {
            get
            {
                return _Language;
            }
            set
            {
                _Language = value;
                _FormatProvider = new CultureInfo(_Language);
            }
        }

        /// <summary>
        /// Gets the Format provider as Culture info from stored language or current culture
        /// </summary>
        /// <value>An CultureInfo</value>
        protected CultureInfo FormatProvider
        {
            get
            {
                if (_FormatProvider == null)
                {
                    _FormatProvider = Thread.CurrentThread.CurrentUICulture;
                }
                return _FormatProvider;
            }
        }

        protected virtual string ReplaceTokens(string strSourceText)
        {
            if (strSourceText == null)
            {
                return string.Empty;
            }
            var Result = new StringBuilder();
            foreach (Match currentMatch in TokenizerRegex.Matches(strSourceText))
            {
                string strObjectName = currentMatch.Result("${object}");
                if (!String.IsNullOrEmpty(strObjectName))
                {
                    if (strObjectName == "[")
                    {
                        strObjectName = ObjectLessToken;
                    }
                    string strPropertyName = currentMatch.Result("${property}");
                    string strFormat = currentMatch.Result("${format}");
                    string strIfEmptyReplacment = currentMatch.Result("${ifEmpty}");
                    string strConversion = replacedTokenValue(strObjectName, strPropertyName, strFormat);
                    if (!String.IsNullOrEmpty(strIfEmptyReplacment) && String.IsNullOrEmpty(strConversion))
                    {
                        strConversion = strIfEmptyReplacment;
                    }
                    Result.Append(strConversion);
                }
                else
                {
                    Result.Append(currentMatch.Result("${text}"));
                }
            }
            return Result.ToString();
        }

        protected abstract string replacedTokenValue(string strObjectName, string strPropertyName, string strFormat);
    }
}
