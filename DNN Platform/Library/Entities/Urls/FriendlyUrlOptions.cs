// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// This class encapsulates different options used in generating friendly urls
    /// </summary>
    [Serializable]
    public class FriendlyUrlOptions
    {
        private static readonly object _regexLookupLock = new object();
        private static readonly Dictionary<string, Regex> _regexLookup = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);

        public bool ConvertDiacriticChars;
        public string IllegalChars;
        public int MaxUrlPathLength;
        public string PageExtension;
        public string PunctuationReplacement;
        //922 : change to use regexMatch pattern for allowable characters
        public string RegexMatch;
        public Dictionary<string, string> ReplaceCharWithChar = new Dictionary<string, string>();
        public string ReplaceChars;
        public bool ReplaceDoubleChars;
        public string SpaceEncoding;

        public bool CanGenerateNonStandardPath
        {
            //replaces statements like this
            //if ((settings.ReplaceSpaceWith != null && settings.ReplaceSpaceWith.Length > 0) || settings.ReplaceCharWithCharDict != null && settings.ReplaceCharWithCharDict.Count > 0)
            get
            {
                bool result = false;
                if (string.IsNullOrEmpty(PunctuationReplacement) == false)
                {
                    result = true;
                }
                else if (ReplaceCharWithChar != null && ReplaceCharWithChar.Count > 0)
                {
                    result = true;
                }
                else if (ConvertDiacriticChars)
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

        public FriendlyUrlOptions Clone()
        {
            var cloned = new FriendlyUrlOptions
                {
                    PunctuationReplacement = PunctuationReplacement,
                    SpaceEncoding = SpaceEncoding,
                    MaxUrlPathLength = MaxUrlPathLength,
                    ConvertDiacriticChars = ConvertDiacriticChars,
                    PageExtension = PageExtension,
                    RegexMatch = RegexMatch,
                    ReplaceCharWithChar = ReplaceCharWithChar,
                    IllegalChars = IllegalChars,
                    ReplaceChars = ReplaceChars
                };
            return cloned;
        }
    }
}
