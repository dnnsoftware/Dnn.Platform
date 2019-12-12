// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IFriendlyUrlWrapper
    {
        string CleanNameForUrl(string urlPath, FriendlyUrlOptions options, out bool modified);
        void ValidateUrl(string urlPath, int tabId, PortalSettings portalSettings, out bool modified);
    }
}
