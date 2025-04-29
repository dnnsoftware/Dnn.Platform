// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups;

using System;
using System.Text.RegularExpressions;

using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;

public class Utilities
{
    public static string NavigateUrl(int tabId, string[] @params)
    {
        return Globals.DependencyProvider.GetRequiredService<INavigationManager>()?.NavigateURL(tabId, string.Empty, @params);
    }

    public static string[] AddParams(string param, string[] currParams)
    {
        var tmpParams = new string[] { param };
        var intLength = tmpParams.Length;
        var currLength = currParams.Length;
        Array.Resize(ref tmpParams, intLength + currLength);
        currParams.CopyTo(tmpParams, intLength);
        return tmpParams;
    }

    internal static string ParseTokenWrapper(string template, string token, bool condition)
    {
        var pattern = "(\\[" + token + "\\](.*?)\\[\\/" + token + "\\])";
        var regExp = RegexUtils.GetCachedRegex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        var matches = regExp.Matches(template);
        foreach (Match match in matches)
        {
            template = template.Replace(match.Value, condition ? match.Groups[2].Value : string.Empty);
        }

        return template;
    }
}
