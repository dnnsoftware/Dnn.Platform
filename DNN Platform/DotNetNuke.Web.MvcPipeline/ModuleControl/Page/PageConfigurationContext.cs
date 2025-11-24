// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Abstractions.ClientResources;
using DotNetNuke.Abstractions.Pages;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Page
{
    public class PageConfigurationContext
    {
        public PageConfigurationContext(IServiceProvider serviceProvider)
        {
            ClientResourceController = serviceProvider.GetService<IClientResourceController>();
            PageService = serviceProvider.GetService<IPageService>();
            ServicesFramework = DotNetNuke.Framework.ServicesFramework.Instance;
            JavaScriptLibraryHelper = serviceProvider.GetService<IJavaScriptLibraryHelper>();
        }

        public IClientResourceController ClientResourceController { get; private set; }

        public IPageService PageService { get; private set; }

        public IServicesFramework ServicesFramework { get; private set; }

        public IJavaScriptLibraryHelper JavaScriptLibraryHelper { get; private set; }
}
}
