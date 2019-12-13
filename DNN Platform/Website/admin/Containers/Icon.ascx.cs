// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.UI.Containers
{
	/// -----------------------------------------------------------------------------
	/// Project	 : DotNetNuke
	/// Class	 : DotNetNuke.UI.Containers.Icon
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
		#region "Public Members"

        public string BorderWidth { get; set; }
        public string CssClass { get; set; }
		
		#endregion

		#region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
				//public attributes
                if (!String.IsNullOrEmpty(BorderWidth))
                {
                    imgIcon.BorderWidth = Unit.Parse(BorderWidth);
                }
                if (!String.IsNullOrEmpty(CssClass))
                {
                    imgIcon.CssClass = CssClass;
                }
                Visible = false;
                if ((ModuleControl != null) && (ModuleControl.ModuleContext.Configuration != null))
                {
                    var iconFile = GetIconFileUrl();
                    if (!string.IsNullOrEmpty(iconFile))
                    {
                        if (Globals.IsAdminControl())
                        {
                            iconFile = $"~/DesktopModules/{ModuleControl.ModuleContext.Configuration.DesktopModule.FolderName}/{iconFile}";
                        }

                        imgIcon.ImageUrl = iconFile;
                        imgIcon.AlternateText = ModuleControl.ModuleContext.Configuration.ModuleTitle;
                        Visible = true;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Private Methods

        private string GetIconFileUrl()
        {
            var iconFile = ModuleControl.ModuleContext.Configuration.IconFile;
            if ((string.IsNullOrEmpty(iconFile) || iconFile.StartsWith("~")))
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
                fileInfo = FileManager.Instance.GetFile(PortalSettings.PortalId, iconFile);
            }

            return fileInfo != null ? FileManager.Instance.GetUrl(fileInfo) : iconFile;
        }

        #endregion
    }
}
