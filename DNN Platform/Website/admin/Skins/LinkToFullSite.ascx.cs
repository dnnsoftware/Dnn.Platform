// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mobile;
    using DotNetNuke.UI.Skins;

    /// -----------------------------------------------------------------------------
    /// <summary>Skin object of portal links between desktop and mobile portals.</summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class LinkToFullSite : SkinObjectBase
    {
        private const string MyFileName = "LinkToFullSite.ascx";

        private string _localResourcesFile;

        private string LocalResourcesFile
        {
            get
            {
                if (string.IsNullOrEmpty(this._localResourcesFile))
                {
                    this._localResourcesFile = Localization.GetResourceFile(this, MyFileName);
                }

                return this._localResourcesFile;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var redirectionController = new RedirectionController();
            var redirectUrl = redirectionController.GetFullSiteUrl();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                this.lnkPortal.NavigateUrl = redirectUrl;
                this.lnkPortal.Text = Localization.GetString("lnkPortal", this.LocalResourcesFile);
            }
            else
            {
                this.Visible = false;
            }
        }
    }
}
