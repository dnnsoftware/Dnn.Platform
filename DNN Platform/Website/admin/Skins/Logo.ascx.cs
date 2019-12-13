#region Usings

using System;
using System.Web.UI.WebControls;
using Microsoft.Extensions.DependencyInjection;

using DotNetNuke.Common;
using DotNetNuke.Abstractions;
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
    /// -----------------------------------------------------------------------------
    public partial class Logo : SkinObjectBase
    {
        private readonly INavigationManager _navigationManager;
        public string BorderWidth { get; set; }
        public string CssClass { get; set; }

        public Logo()
        {
            _navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!String.IsNullOrEmpty(BorderWidth))
                {
                    imgLogo.BorderWidth = Unit.Parse(BorderWidth);
                }
                if (!String.IsNullOrEmpty(CssClass))
                {
                    imgLogo.CssClass = CssClass;
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

                if (!imgLogo.Visible)
                {
                    hypLogo.Attributes.Add("aria-label", PortalSettings.PortalName);
                }
                if (PortalSettings.HomeTabId != -1)
                {
                    hypLogo.NavigateUrl = _navigationManager.NavigateURL(PortalSettings.HomeTabId);
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
