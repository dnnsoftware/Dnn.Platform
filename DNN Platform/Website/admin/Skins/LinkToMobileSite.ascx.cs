// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mobile;
    using DotNetNuke.UI.Skins;

    /// <summary>Skin object of portal links between desktop and mobile portals.</summary>
    public partial class LinkToMobileSite : SkinObjectBase
    {
        private const string MyFileName = "LinkToMobileSite.ascx";

        private string localResourcesFile;

        private string LocalResourcesFile
        {
            get
            {
                if (string.IsNullOrEmpty(this.localResourcesFile))
                {
                    this.localResourcesFile = Localization.GetResourceFile(this, MyFileName);
                }

                return this.localResourcesFile;
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var redirectionController = new RedirectionController();
            var redirectUrl = redirectionController.GetMobileSiteUrl();
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
