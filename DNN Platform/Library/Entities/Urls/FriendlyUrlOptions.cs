// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;

    /// <summary>
    /// This class encapsulates different options used in generating friendly urls.
    /// </summary>
    [Serializable]
    public class FriendlyUrlOptions
    {
        public bool ConvertDiacriticChars;
        public string IllegalChars;
        public int MaxUrlPathLength;
        public string PageExtension;
        public string PunctuationReplacement;

        // 922 : change to use regexMatch pattern for allowable characters
        public string RegexMatch;
        public Dictionary<string, string> ReplaceCharWithChar = new Dictionary<string, string>();
        public string ReplaceChars;
        public bool ReplaceDoubleChars;
        public string SpaceEncoding;
        private static readonly object _regexLookupLock = new object();
        private static readonly Dictionary<string, Regex> _regexLookup = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);

        public bool CanGenerateNonStandardPath
        {
            // replaces statements like this
            // if ((settings.ReplaceSpaceWith != null && settings.ReplaceSpaceWith.Length > 0) || settings.ReplaceCharWithCharDict != null && settings.ReplaceCharWithCharDict.Count > 0)
            get
            {
                bool result = false;
                if (string.IsNullOrEmpty(this.PunctuationReplacement) == false)
                {
                    result = true;
                }
                else if (this.ReplaceCharWithChar != null && this.ReplaceCharWithChar.Count > 0)
                {
                    result = true;
                }
                else if (this.ConvertDiacriticChars)
                {
                    result = true;
                }

                return result;
            }
        }

        public Regex RegexMatchRegex
        {
            get { return GetRegex(this.RegexMatch); }
        }

        public FriendlyUrlOptions Clone()
        {
            var cloned = new FriendlyUrlOptions
            {
                PunctuationReplacement = this.PunctuationReplacement,
                SpaceEncoding = this.SpaceEncoding,
                MaxUrlPathLength = this.MaxUrlPathLength,
                ConvertDiacriticChars = this.ConvertDiacriticChars,
                PageExtension = this.PageExtension,
                RegexMatch = this.RegexMatch,
                ReplaceCharWithChar = this.ReplaceCharWithChar,
                IllegalChars = this.IllegalChars,
                ReplaceChars = this.ReplaceChars,
            };
            return cloned;
        }

        private static Regex GetRegex(string regexText)
        {
            Regex compiledRegex;
            if (_regexLookup.TryGetValue(regexText, out compiledRegex))
            {
                return compiledRegex;
            }

            lock (_regexLookupLock)
            {
                if (_regexLookup.TryGetValue(regexText, out compiledRegex))
                {
                    return compiledRegex;
                }

                return _regexLookup[regexText] = RegexUtils.GetCachedRegex(regexText, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
        }
    }
}
