// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    public class PortalModel: PortalModelBase
    {
        public int CdfVersion { get; set; }
        public string SiteTheme { get; set; }
        public string AdminTheme { get; set; }
        public string Container { get; set; }
        public string AdminContainer { get; set; }

        #region Command Links
        public string __PageCount => "list-pages";

        public string __UserCount => "list-users";

        #endregion

        #region Constructors
        public PortalModel()
        {
        }
        public PortalModel(PortalInfo portal): base(portal)
        {
            // get portal settings for specified portal
            var ps = new PortalSettings(portal);
            CdfVersion = ps.CdfVersion;
            SiteTheme = Utilities.FormatSkinName(ps.DefaultPortalSkin);
            AdminTheme = Utilities.FormatSkinName(ps.DefaultAdminSkin);
            Container = Utilities.FormatContainerName(ps.DefaultPortalContainer);
            AdminContainer = Utilities.FormatContainerName(ps.DefaultAdminContainer);
        }
        #endregion

    }
}
