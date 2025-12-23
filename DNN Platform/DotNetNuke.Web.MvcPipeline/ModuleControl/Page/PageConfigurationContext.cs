// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Page
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides services and helpers for configuring page-level resources for MVC modules.
    /// </summary>
    public class PageConfigurationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageConfigurationContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The dependency injection service provider.</param>
        public PageConfigurationContext(IServiceProvider serviceProvider)
        {
            this.ClientResourceController = serviceProvider.GetService<IClientResourceController>();
            this.PageService = serviceProvider.GetService<IPageService>();
            this.ServicesFramework = DotNetNuke.Framework.ServicesFramework.Instance;
            this.JavaScriptLibraryHelper = serviceProvider.GetService<IJavaScriptLibraryHelper>();
        }

        /// <summary>
        /// Gets the client resource controller used to register scripts and styles.
        /// </summary>
        public IClientResourceController ClientResourceController { get; private set; }

        /// <summary>
        /// Gets the page service used to manage page metadata.
        /// </summary>
        public IPageService PageService { get; private set; }

        /// <summary>
        /// Gets the DNN services framework instance.
        /// </summary>
        public IServicesFramework ServicesFramework { get; private set; }

        /// <summary>
        /// Gets the JavaScript library helper used to request JavaScript libraries.
        /// </summary>
        public IJavaScriptLibraryHelper JavaScriptLibraryHelper { get; private set; }
    }
}
