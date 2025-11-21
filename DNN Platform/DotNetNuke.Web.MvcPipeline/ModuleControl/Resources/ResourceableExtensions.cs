// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Web.UI;
using DotNetNuke.Abstractions.ClientResources;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.ClientDependency;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Resources;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{


    /// <summary>
    /// Extension methods for IResourceable interface.
    /// </summary>
    public static class ResourceableExtensions
    {
        public static void RegisterResources(this IResourcable resourcable, IClientResourceController clientResourceController)
        {
            if (resourcable.ModuleResources.StyleSheets != null)
            {
                foreach (var styleSheet in resourcable.ModuleResources.StyleSheets)
                {
                    clientResourceController.RegisterStylesheet(styleSheet.FilePath, styleSheet.Priority);
                }
            }
            if (resourcable.ModuleResources.Scripts != null)
            {
                foreach (var javaScript in resourcable.ModuleResources.Scripts)
                {
                    clientResourceController.RegisterScript(javaScript.FilePath, javaScript.Priority);
                }
            }
            if (resourcable.ModuleResources.Libraries != null)
            {
                foreach (var lib in resourcable.ModuleResources.Libraries)
                {
                    JavaScript.RequestRegistration(lib);
                }
            }
            if (resourcable.ModuleResources.AjaxScript)
            {
                ServicesFramework.Instance.RequestAjaxScriptSupport();
            }
            if (resourcable.ModuleResources.AjaxAntiForgery)
            {
                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            }
        }
    }
}
