// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Urls;

    public class FriendlyUrlWrapper : IFriendlyUrlWrapper
    {
        public string CleanNameForUrl(string urlPath, FriendlyUrlOptions options, out bool modified)
        {
            return FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
        }

        public void ValidateUrl(string urlPath, int tabld, PortalSettings portalSettings, out bool modified)
        {
            FriendlyUrlController.ValidateUrl(urlPath, tabld, portalSettings, out modified);
        }
    }
}
