// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Creates <see cref="PageModel"/> instances for DNN MVC pages.
    /// </summary>
    public interface IPageModelFactory
    {
        /// <summary>
        /// Creates a page model for the specified page controller.
        /// </summary>
        /// <param name="page">The DNN MVC page controller.</param>
        /// <returns>A populated <see cref="PageModel"/>.</returns>
        PageModel CreatePageModel(DnnPageController page);
    }
}
