// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Represents the result of executing a Razor-based module control.
    /// </summary>
    public interface IRazorModuleResult
    {
        /// <summary>
        /// Executes the result using the specified HTML helper.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The rendered HTML.</returns>
        IHtmlString Execute(HtmlHelper htmlHelper);
    }
}
