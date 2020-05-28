// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Services.Cache;

namespace DotNetNuke.UI.Modules.Html5
{
    public class Html5HostControl : ModuleControlBase, IActionable
    {
        private readonly string _html5File;
        private string _fileContent;

        public Html5HostControl(string html5File)
        {
            _html5File = html5File;
        }

        public ModuleActionCollection ModuleActions { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!(string.IsNullOrEmpty(_html5File)))
            {
                //Check if css file exists
                var cssFile = Path.ChangeExtension(_html5File, ".css");
                if (FileExists(cssFile))
                {
                    ClientResourceManager.RegisterStyleSheet(Page, cssFile, FileOrder.Css.DefaultPriority);
                }

                //Check if js file exists
                var jsFile = Path.ChangeExtension(_html5File, ".js");
                if (FileExists(jsFile))
                {
                    ClientResourceManager.RegisterScript(Page, jsFile, FileOrder.Js.DefaultPriority);
                }

                _fileContent = GetFileContent(_html5File);

                ModuleActions = new ModuleActionCollection();
                var tokenReplace = new Html5ModuleTokenReplace(Page, _html5File, ModuleContext, ModuleActions);
                _fileContent = tokenReplace.ReplaceEnvironmentTokens(_fileContent);
            }

            //Register for Services Framework
            ServicesFramework.Instance.RequestAjaxScriptSupport();
        }

        private string GetFileContent(string filepath)
        {
            var cacheKey = string.Format(DataCache.SpaModulesContentHtmlFileCacheKey, filepath);
            var absoluteFilePath = Page.Server.MapPath(filepath);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.SpaModulesHtmlFileTimeOut,
                DataCache.SpaModulesHtmlFileCachePriority)
            {
                CacheDependency = new DNNCacheDependency(absoluteFilePath)
            };
            return CBO.GetCachedObject<string>(cacheItemArgs, c => GetFileContentInternal(absoluteFilePath));
        }

        private bool FileExists(string filepath)
        {
            var cacheKey = string.Format(DataCache.SpaModulesFileExistsCacheKey, filepath);
            return CBO.GetCachedObject<bool>(new CacheItemArgs(cacheKey,
                DataCache.SpaModulesHtmlFileTimeOut,
                DataCache.SpaModulesHtmlFileCachePriority), 
                c => File.Exists(Page.Server.MapPath(filepath)));
        }

        private static string GetFileContentInternal(string filepath)
        {
            using (var reader = new StreamReader(filepath))
            {
                return reader.ReadToEnd();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!(string.IsNullOrEmpty(_html5File)))
            {
                Controls.Add(new LiteralControl(HttpUtility.HtmlDecode(_fileContent)));
            }
        }
    }
}
