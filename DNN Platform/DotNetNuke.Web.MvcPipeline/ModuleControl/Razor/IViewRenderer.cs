// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    /// <summary>
    /// Defines a service that can render MVC views to strings.
    /// </summary>
    public interface IViewRenderer
    {
        /// <summary>
        /// Renders the specified view to a string.
        /// </summary>
        /// <param name="viewPath">The virtual path or name of the view.</param>
        /// <param name="model">The model to pass to the view.</param>
        /// <returns>The rendered view as a string.</returns>
        string RenderViewToString(string viewPath, object model = null);
    }
}
