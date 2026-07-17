// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    /// <summary>
    /// Razor result that renders a partial view with an optional model and view data.
    /// </summary>
    public class ViewRazorModuleResult : IRazorModuleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRazorModuleResult"/> class.
        /// </summary>
        /// <param name="viewName">The name or path of the view.</param>
        /// <param name="model">The model to pass to the view.</param>
        /// <param name="viewData">The view data dictionary.</param>
        public ViewRazorModuleResult(string viewName, object model, ViewDataDictionary viewData)
        {
            this.ViewName = viewName;
            this.Model = model;
            this.ViewData = viewData;
        }

        /// <summary>
        /// Gets the view name or path.
        /// </summary>
        public string ViewName { get; private set; }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        public object Model { get; private set; }

        /// <summary>
        /// Gets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        public ViewDataDictionary ViewData { get; private set; }

        /// <inheritdoc/>
        public IHtmlString Execute(HtmlHelper htmlHelper)
        {
            return htmlHelper.Partial(this.ViewName, this.Model, this.ViewData);
        }
    }
}
