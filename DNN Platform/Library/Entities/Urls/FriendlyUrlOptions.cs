// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using DotNetNuke.Common.Utilities;

/// <summary>This class encapsulates different options used in generating friendly urls.</summary>
[Serializable]
public class FriendlyUrlOptions
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public bool ConvertDiacriticChars;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string IllegalChars;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public int MaxUrlPathLength;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string PageExtension;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string PunctuationReplacement;

    // 922 : change to use regexMatch pattern for allowable characters
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string RegexMatch;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public Dictionary<string, string> ReplaceCharWithChar = new Dictionary<string, string>();
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string ReplaceChars;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public bool ReplaceDoubleChars;
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string SpaceEncoding;

    private static readonly object RegexLookupLock = new object();
    private static readonly Dictionary<string, Regex> RegexLookup = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);

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
        if (RegexLookup.TryGetValue(regexText, out compiledRegex))
        {
            return compiledRegex;
        }

        lock (RegexLookupLock)
        {
            if (RegexLookup.TryGetValue(regexText, out compiledRegex))
            {
                return compiledRegex;
            }

            return RegexLookup[regexText] = RegexUtils.GetCachedRegex(regexText, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
    }
}
