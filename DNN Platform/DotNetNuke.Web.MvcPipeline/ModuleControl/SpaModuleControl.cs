// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.UI;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Containers;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Modules.Html5;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Resources;
    using DotNetNuke.Web.MvcPipeline.Tokens;
    using Microsoft.Extensions.DependencyInjection;

    public class SpaModuleControl : DefaultMvcModuleControlBase, IResourcable
    {
        private readonly IBusinessControllerProvider businessControllerProvider;

        public SpaModuleControl(): base()
        {
            this.businessControllerProvider = Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
        }

        public SpaModuleControl(IBusinessControllerProvider businessControllerProvider) : base()
        {
            this.businessControllerProvider = businessControllerProvider;
        }

        public string html5File => ModuleConfiguration.ModuleControl.ControlSrc;

        public ModuleResources ModuleResources
        {
            get
            {
                var resource = new ModuleResources()
                {
                    // Register for Services Framework
                    AjaxScript = true
                };

                if (!string.IsNullOrEmpty(this.html5File))
                {
                    // Check if css file exists
                    var cssFile = Path.ChangeExtension(this.html5File, ".css");
                    if (this.FileExists(cssFile))
                    {
                        resource.StyleSheets.Add(new ModuleStyleSheet()
                        {
                            FilePath = "/" + cssFile,
                            Priority = FileOrder.Css.DefaultPriority,
                        });
                    }
                }

                if (!string.IsNullOrEmpty(this.html5File))
                {
                    // Check if js file exists
                    var jsFile = Path.ChangeExtension(this.html5File, ".js");
                    if (this.FileExists(jsFile))
                    {
                        resource.Scripts.Add(new ModuleScript()
                        {
                            FilePath = "/" + jsFile,
                            Priority = FileOrder.Js.DefaultPriority,
                        });
                    }
                }
                return resource;
            }
        }
        public override IHtmlString Html(HtmlHelper htmlHelper)
        {
            var fileContent = string.Empty;
            if (!string.IsNullOrEmpty(this.html5File))
            {
                fileContent = this.GetFileContent(this.html5File);
                var ModuleActions = new ModuleActionCollection();
                var tokenReplace = new MvcHtml5ModuleTokenReplace(htmlHelper.ViewContext, this.businessControllerProvider, this.html5File, this.ModuleContext, ModuleActions);
                fileContent = tokenReplace.ReplaceEnvironmentTokens(fileContent);
            }
                        
            return new HtmlString(HttpUtility.HtmlDecode(fileContent));
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
            var absoluteFilePath = HttpContext.Current.Server.MapPath("/" + filepath);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.SpaModulesHtmlFileTimeOut, DataCache.SpaModulesHtmlFileCachePriority)
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
                c => File.Exists(HttpContext.Current.Server.MapPath("/" + filepath)));
        }
    }
}
