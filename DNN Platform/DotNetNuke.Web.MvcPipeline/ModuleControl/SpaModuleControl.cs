// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System.IO;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.Modules.Html5;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// SPA-style HTML5 module control that renders content from an HTML file and processes DNN tokens.
    /// </summary>
    public class SpaModuleControl : DefaultMvcModuleControlBase, IPageContributor
    {
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpaModuleControl"/> class using the global dependency provider.
        /// </summary>
        public SpaModuleControl()
            : base()
        {
            this.businessControllerProvider = Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            var serviceProvider = Common.Globals.GetCurrentServiceProvider();
            this.clientResourceController = serviceProvider.GetRequiredService<IClientResourceController>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpaModuleControl"/> class with explicit dependencies.
        /// </summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public SpaModuleControl(IBusinessControllerProvider businessControllerProvider, IClientResourceController clientResourceController)
            : base()
        {
            this.businessControllerProvider = businessControllerProvider;
            this.clientResourceController = clientResourceController;
        }

        /// <summary>
        /// Gets the HTML5 control source file for this module.
        /// </summary>
        public string Html5File => this.ModuleConfiguration.ModuleControl.ControlSrc;

        /// <summary>
        /// Configures page resources required by the SPA module (scripts and styles).
        /// </summary>
        /// <param name="context">The page configuration context.</param>
        public void ConfigurePage(PageConfigurationContext context)
        {
            context.ServicesFramework.RequestAjaxScriptSupport();
            if (!string.IsNullOrEmpty(this.Html5File))
            {
                // Check if css file exists
                var cssFile = Globals.ApplicationPath + "/" + Path.ChangeExtension(this.Html5File, ".css");
                context.ClientResourceController.RegisterStylesheet(cssFile, true);
            }

            if (!string.IsNullOrEmpty(this.Html5File))
            {
                // Check if js file exists
                var jsFile = Globals.ApplicationPath + "/" + Path.ChangeExtension(this.Html5File, ".js");
                context.ClientResourceController.RegisterScript(jsFile, true);
            }
        }

        /// <summary>
        /// Renders the SPA module by loading and token-replacing the HTML5 file content.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The rendered HTML.</returns>
        public override IHtmlString Html(HtmlHelper htmlHelper)
        {
            var fileContent = string.Empty;
            if (!string.IsNullOrEmpty(this.Html5File))
            {
                fileContent = this.GetFileContent(this.Html5File);
                var moduleActions = new ModuleActionCollection();
                var tokenReplace = new Html5ModuleTokenReplace(null, htmlHelper.ViewContext.HttpContext.Request, this.clientResourceController, this.businessControllerProvider, this.Html5File, this.ModuleContext, moduleActions);
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
            var absoluteFilePath = HttpContext.Current.Server.MapPath(Globals.ApplicationPath + "/" + filepath);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.SpaModulesHtmlFileTimeOut, DataCache.SpaModulesHtmlFileCachePriority)
            {
                CacheDependency = new DNNCacheDependency(absoluteFilePath),
            };
            return CBO.GetCachedObject<string>(cacheItemArgs, c => GetFileContentInternal(absoluteFilePath));
        }
    }
}
