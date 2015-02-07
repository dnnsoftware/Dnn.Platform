#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// <history>
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Logo : SkinObjectBase
    {
        public string BorderWidth { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!String.IsNullOrEmpty(BorderWidth))
                {
                    imgLogo.BorderWidth = Unit.Parse(BorderWidth);
                }
                bool logoVisible = false;
                if (!String.IsNullOrEmpty(PortalSettings.LogoFile))
                {
                    var fileInfo = GetLogoFileInfo();
                    if (fileInfo != null)
                    {
                        string imageUrl = FileManager.Instance.GetUrl(fileInfo);
                        if (!String.IsNullOrEmpty(imageUrl))
                        {
                            imgLogo.ImageUrl = imageUrl;
                            logoVisible = true;
                        }
                    }
                }
                imgLogo.Visible = logoVisible;
                imgLogo.AlternateText = PortalSettings.PortalName;
                hypLogo.ToolTip = PortalSettings.PortalName;
                if (PortalSettings.HomeTabId != -1)
                {
                    hypLogo.NavigateUrl = Globals.NavigateURL(PortalSettings.HomeTabId);
                }
                else
                {
                    hypLogo.NavigateUrl = Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private IFileInfo GetLogoFileInfo()
        {
            string cacheKey = String.Format(DataCache.PortalCacheKey, PortalSettings.PortalId, PortalSettings.CultureCode) + "LogoFile";
            var file = CBO.GetCachedObject<FileInfo>(new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority),
                                                    GetLogoFileInfoCallBack);

            return file;
        }

        private IFileInfo GetLogoFileInfoCallBack(CacheItemArgs itemArgs)
        {
            return FileManager.Instance.GetFile(PortalSettings.PortalId, PortalSettings.LogoFile);
        }
    }
}