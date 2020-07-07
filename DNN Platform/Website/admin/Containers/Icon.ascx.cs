// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.UI.Skins;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : DotNetNuke.UI.Containers.Icon
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Contains the attributes of an Icon.
    /// These are read into the PortalModuleBase collection as attributes for the icons within the module controls.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class Icon : SkinObjectBase
    {
        public string BorderWidth { get; set; }

        public string CssClass { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                // public attributes
                if (!string.IsNullOrEmpty(this.BorderWidth))
                {
                    this.imgIcon.BorderWidth = Unit.Parse(this.BorderWidth);
                }

                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.imgIcon.CssClass = this.CssClass;
                }

                this.Visible = false;
                if ((this.ModuleControl != null) && (this.ModuleControl.ModuleContext.Configuration != null))
                {
                    var iconFile = this.GetIconFileUrl();
                    if (!string.IsNullOrEmpty(iconFile))
                    {
                        if (Globals.IsAdminControl())
                        {
                            iconFile = $"~/DesktopModules/{this.ModuleControl.ModuleContext.Configuration.DesktopModule.FolderName}/{iconFile}";
                        }

                        this.imgIcon.ImageUrl = iconFile;
                        this.imgIcon.AlternateText = this.ModuleControl.ModuleContext.Configuration.ModuleTitle;
                        this.Visible = true;
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private string GetIconFileUrl()
        {
            var iconFile = this.ModuleControl.ModuleContext.Configuration.IconFile;
            if (string.IsNullOrEmpty(iconFile) || iconFile.StartsWith("~"))
            {
                return iconFile;
            }

            IFileInfo fileInfo;
            if (iconFile.StartsWith("FileID=", StringComparison.InvariantCultureIgnoreCase))
            {
                var fileId = Convert.ToInt32(iconFile.Substring(7));
                fileInfo = FileManager.Instance.GetFile(fileId);
            }
            else
            {
                fileInfo = FileManager.Instance.GetFile(this.PortalSettings.PortalId, iconFile);
            }

            return fileInfo != null ? FileManager.Instance.GetUrl(fileInfo) : iconFile;
        }
    }
}
