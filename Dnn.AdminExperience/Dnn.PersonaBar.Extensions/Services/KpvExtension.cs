// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services;

using System.Collections.Generic;
using System.Linq;

using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common;

internal static class KpvExtension
{
    public static IEnumerable<LocalizationEntry> MapEntries(this IEnumerable<KeyValuePair<string, string>> list)
    {
        var appPath = Globals.ApplicationMapPath;
        var appPathLen = appPath.Length;
        if (!appPath.EndsWith(@"\"))
        {
            appPathLen++;
        }

        return list.Select(kpv => new LocalizationEntry
        {
            Name = kpv.Key,
            NewValue = (kpv.Value.StartsWith(appPath) ? kpv.Value.Substring(appPathLen) : kpv.Value).Replace(@"\", @"/"),
        });
    }
}
