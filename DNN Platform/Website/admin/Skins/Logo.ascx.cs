// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Logo : SkinObjectBase
    {
        private readonly INavigationManager _navigationManager;

        public Logo()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string BorderWidth { get; set; }

        public string CssClass { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.BorderWidth))
                {
                    this.imgLogo.BorderWidth = Unit.Parse(this.BorderWidth);
                }

                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.imgLogo.CssClass = this.CssClass;
                }

                bool logoVisible = false;
                if (!string.IsNullOrEmpty(this.PortalSettings.LogoFile))
                {
                    var fileInfo = this.GetLogoFileInfo();
                    if (fileInfo != null)
                    {
                        string imageUrl = FileManager.Instance.GetUrl(fileInfo);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            this.imgLogo.ImageUrl = imageUrl;
                            logoVisible = true;
                        }
                    }
                }

                this.imgLogo.Visible = logoVisible;
                this.imgLogo.AlternateText = this.PortalSettings.PortalName;
                this.hypLogo.ToolTip = this.PortalSettings.PortalName;

                if (!this.imgLogo.Visible)
                {
                    this.hypLogo.Attributes.Add("aria-label", this.PortalSettings.PortalName);
                }

                if (this.PortalSettings.HomeTabId != -1)
                {
                    this.hypLogo.NavigateUrl = this._navigationManager.NavigateURL(this.PortalSettings.HomeTabId);
                }
                else
                {
                    this.hypLogo.NavigateUrl = Globals.AddHTTP(this.PortalSettings.PortalAlias.HTTPAlias);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private IFileInfo GetLogoFileInfo()
        {
            string cacheKey = string.Format(DataCache.PortalCacheKey, this.PortalSettings.PortalId, this.PortalSettings.CultureCode) + "LogoFile";
            var file = CBO.GetCachedObject<FileInfo>(
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority),
                this.GetLogoFileInfoCallBack);

            return file;
        }

        private IFileInfo GetLogoFileInfoCallBack(CacheItemArgs itemArgs)
        {
            return FileManager.Instance.GetFile(this.PortalSettings.PortalId, this.PortalSettings.LogoFile);
        }
    }
}
