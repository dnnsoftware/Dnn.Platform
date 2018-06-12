#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
