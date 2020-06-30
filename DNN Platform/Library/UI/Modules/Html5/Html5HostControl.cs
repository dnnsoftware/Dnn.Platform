// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules.Html5
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public class Html5HostControl : ModuleControlBase, IActionable
    {
        private readonly string _html5File;
        private string _fileContent;

        public Html5HostControl(string html5File)
        {
            this._html5File = html5File;
        }

        public ModuleActionCollection ModuleActions { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!string.IsNullOrEmpty(this._html5File))
            {
                // Check if css file exists
                var cssFile = Path.ChangeExtension(this._html5File, ".css");
                if (this.FileExists(cssFile))
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, cssFile, FileOrder.Css.DefaultPriority);
                }

                // Check if js file exists
                var jsFile = Path.ChangeExtension(this._html5File, ".js");
                if (this.FileExists(jsFile))
                {
                    ClientResourceManager.RegisterScript(this.Page, jsFile, FileOrder.Js.DefaultPriority);
                }

                this._fileContent = this.GetFileContent(this._html5File);

                this.ModuleActions = new ModuleActionCollection();
                var tokenReplace = new Html5ModuleTokenReplace(this.Page, this._html5File, this.ModuleContext, this.ModuleActions);
                this._fileContent = tokenReplace.ReplaceEnvironmentTokens(this._fileContent);
            }

            // Register for Services Framework
            ServicesFramework.Instance.RequestAjaxScriptSupport();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(this._html5File))
            {
                this.Controls.Add(new LiteralControl(HttpUtility.HtmlDecode(this._fileContent)));
            }
        }

        private static string GetFileContentInternal(string filepath)
        {
            using (var reader = new StreamReader(filepath))
            {
                return reader.ReadToEnd();
            }
        }

        private string GetFileContent(string filepath)
        {
            var cacheKey = string.Format(DataCache.SpaModulesContentHtmlFileCacheKey, filepath);
            var absoluteFilePath = this.Page.Server.MapPath(filepath);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.SpaModulesHtmlFileTimeOut,
                DataCache.SpaModulesHtmlFileCachePriority)
            {
                CacheDependency = new DNNCacheDependency(absoluteFilePath),
            };
            return CBO.GetCachedObject<string>(cacheItemArgs, c => GetFileContentInternal(absoluteFilePath));
        }

        private bool FileExists(string filepath)
        {
            var cacheKey = string.Format(DataCache.SpaModulesFileExistsCacheKey, filepath);
            return CBO.GetCachedObject<bool>(
                new CacheItemArgs(
                cacheKey,
                DataCache.SpaModulesHtmlFileTimeOut,
                DataCache.SpaModulesHtmlFileCachePriority),
                c => File.Exists(this.Page.Server.MapPath(filepath)));
        }
    }
}
