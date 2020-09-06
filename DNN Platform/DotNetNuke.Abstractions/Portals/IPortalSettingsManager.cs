// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Portals
{
    using DotNetNuke.Abstractions.Settings;

    public interface IPortalSettingsManager
    {
        ISettingsService GetPortalSettings(int portalId);

        ISettingsService GetPortalSettings(int portalId, string cultureCode);
    }
}
