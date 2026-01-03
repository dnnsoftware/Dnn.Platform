// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Page
{
    /// <summary>
    /// Defines a contract for module controls that need to contribute resources to the page.
    /// </summary>
    public interface IPageContributor
    {
        /// <summary>
        /// Configures page-level resources such as scripts and styles for the module.
        /// </summary>
        /// <param name="context">The page configuration context.</param>
        void ConfigurePage(PageConfigurationContext context);
    }
}
