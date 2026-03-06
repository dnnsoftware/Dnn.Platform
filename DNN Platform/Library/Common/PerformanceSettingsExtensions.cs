// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common;

using NewPerformanceSettings = DotNetNuke.Abstractions.Application.PerformanceSettings;
using OldPerformanceSettings = DotNetNuke.Common.Globals.PerformanceSettings;

public static class PerformanceSettingsExtensions
{
    /// <summary>Converts a <see cref="NewPerformanceSettings"/> value into a <see cref="OldPerformanceSettings"/> value.</summary>
    /// <param name="newValue">The new value.</param>
    /// <returns>The old value.</returns>
    public static OldPerformanceSettings ToOldEnum(this NewPerformanceSettings newValue)
    {
        return newValue switch
        {
            NewPerformanceSettings.NoCaching => OldPerformanceSettings.NoCaching,
            NewPerformanceSettings.LightCaching => OldPerformanceSettings.LightCaching,
            NewPerformanceSettings.ModerateCaching => OldPerformanceSettings.ModerateCaching,
            NewPerformanceSettings.HeavyCaching => OldPerformanceSettings.HeavyCaching,
            _ => (OldPerformanceSettings)newValue,
        };
    }

    /// <summary>Converts a <see cref="OldPerformanceSettings"/> value into a <see cref="NewPerformanceSettings"/> value.</summary>
    /// <param name="oldValue">The old value.</param>
    /// <returns>The new value.</returns>
    public static NewPerformanceSettings ToNewEnum(this OldPerformanceSettings oldValue)
    {
        return oldValue switch
        {
            OldPerformanceSettings.NoCaching => NewPerformanceSettings.NoCaching,
            OldPerformanceSettings.LightCaching => NewPerformanceSettings.LightCaching,
            OldPerformanceSettings.ModerateCaching => NewPerformanceSettings.ModerateCaching,
            OldPerformanceSettings.HeavyCaching => NewPerformanceSettings.HeavyCaching,
            _ => (NewPerformanceSettings)oldValue,
        };
    }
}
