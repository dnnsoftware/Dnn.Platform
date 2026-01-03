// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Routing;

    using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
    using DotNetNuke.Web.MvcPipeline.Modules;

    /// <summary>
    /// Base class for Razor-based MVC module controls.
    /// </summary>
    public abstract class RazorModuleControlBase : DefaultMvcModuleControlBase
    {
        private RazorModuleViewContext viewContext;

        /// <summary>
        /// Gets or sets the Razor module view context.
        /// </summary>
        public RazorModuleViewContext ViewContext
        {
            get
            {
                if (this.viewContext == null)
                {
                    this.viewContext = new RazorModuleViewContext
                    {
                        HttpContext = new HttpContextWrapper(System.Web.HttpContext.Current),
                    };
                }

                return this.viewContext;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.viewContext = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Http.HttpContext"/>.
        /// </summary>
        public HttpContextBase HttpContext => this.ViewContext.HttpContext;

        /// <summary>
        /// Gets the <see cref="HttpRequest"/>.
        /// </summary>
        public HttpRequestBase Request => this.ViewContext.HttpContext.Request;

        /// <summary>
        /// Gets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        public ViewDataDictionary ViewData => this.ViewContext.ViewData;

        /// <summary>
        /// Gets the default view path for this module control.
        /// </summary>
        protected virtual string DefaultViewName
        {
            get
            {
                return "~/" + this.ControlPath.Replace('\\', '/').Trim('/') + "/Views/" + this.ControlName + ".cshtml";
            }
        }

        /// <summary>
        /// Renders the module control using Razor and returns the resulting HTML.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The rendered HTML.</returns>
        public override IHtmlString Html(HtmlHelper htmlHelper)
        {
            this.ViewContext.ViewData = new ViewDataDictionary(htmlHelper.ViewData);
            this.ViewContext.ViewData["ModuleContext"] = this.ModuleContext;
            this.ViewContext.ViewData["ModuleId"] = this.ModuleId;
            this.ViewContext.ViewData["LocalResourceFile"] = this.LocalResourceFile;
            var res = this.Invoke();
            return res.Execute(htmlHelper);
        }

        /// <summary>
        /// Executes the module and returns a Razor module result to be rendered.
        /// </summary>
        /// <returns>The Razor module result.</returns>
        public abstract IRazorModuleResult Invoke();

        /// <summary>
        /// Returns a result which will render HTML encoded text.
        /// </summary>
        /// <param name="content">The content, will be HTML encoded before output.</param>
        /// <returns>A <see cref="IRazorModuleResult"/>.</returns>
        public IRazorModuleResult Content(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            return new ContentRazorModuleResult(content);
        }

        /// <summary>
        /// Returns a result which will render an error message.
        /// </summary>
        /// <param name="heading">The error heading.</param>
        /// <param name="message">The error message.</param>
        /// <returns>A <see cref="IRazorModuleResult"/>.</returns>
        public IRazorModuleResult Error(string heading, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            return new ErrorRazorModuleResult(heading, message);
        }

        /// <summary>
        /// Returns a result that renders the default view.
        /// </summary>
        /// <returns>A <see cref="IRazorModuleResult"/>.</returns>
        public IRazorModuleResult View()
        {
            return this.View(null);
        }

        /// <summary>
        /// Returns a result that renders the specified view.
        /// </summary>
        /// <param name="viewName">The view name.</param>
        /// <returns>A <see cref="IRazorModuleResult"/>.</returns>
        public IRazorModuleResult View(string viewName)
        {
            return this.View(viewName, null);
        }

        /// <summary>
        /// Returns a result that renders the default view with the specified model.
        /// </summary>
        /// <param name="model">The view model.</param>
        /// <returns>A <see cref="IRazorModuleResult"/>.</returns>
        public IRazorModuleResult View(object model)
        {
            return this.View(null, model);
        }

        /// <summary>
        /// Returns a result that renders the specified view with the specified model.
        /// </summary>
        /// <param name="viewName">The view name.</param>
        /// <param name="model">The view model.</param>
        /// <returns>A <see cref="IRazorModuleResult"/>.</returns>
        public IRazorModuleResult View(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = this.DefaultViewName;
            }

            return new ViewRazorModuleResult(viewName, model, this.ViewData);
        }
    }
}
