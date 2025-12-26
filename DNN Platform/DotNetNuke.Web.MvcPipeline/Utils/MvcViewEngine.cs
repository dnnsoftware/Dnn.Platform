// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Utils
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Renders MVC views and HtmlHelper output to strings or writers using the standard MVC view engine.
    /// Requires an ASP.NET <see cref="HttpContext"/> to be present, but can be used outside of a controller.
    /// </summary>
    public class MvcViewEngine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MvcViewEngine"/> class.
        /// </summary>
        /// <param name="controllerContext">
        /// If you are running within the context of an ASP.NET MVC request, pass in
        /// the controller's context. Only leave out the context if no context is otherwise available.
        /// </param>
        public MvcViewEngine(ControllerContext controllerContext = null)
        {
            // Create a known controller from HttpContext if no context is passed.
            if (controllerContext == null)
            {
                if (HttpContext.Current != null)
                {
                    controllerContext = CreateController<EmptyController>().ControllerContext;
                }
                else
                {
                    throw new InvalidOperationException(
                        "ViewRenderer must run in the context of an ASP.NET " +
                        "Application and requires HttpContext.Current to be present.");
                }
            }

            this.Context = controllerContext;
        }

        /// <summary>
        /// Gets or sets the current controller context used for rendering.
        /// </summary>
        protected ControllerContext Context { get; set; }

        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by
        /// name, or as fully qualified ~/ path including extension.
        /// </param>
        /// <param name="model">The model to pass to the view renderer.</param>
        /// <param name="controllerContext">Active controller context.</param>
        /// <returns>String of the rendered view or <c>null</c> on error.</returns>
        public static string RenderPartialView(string viewPath, object model = null, ControllerContext controllerContext = null)
        {
            var renderer = new MvcViewEngine(controllerContext);
            return renderer.RenderPartialViewToString(viewPath, model);
        }

        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by
        /// name, or as fully qualified ~/ path including extension.
        /// </param>
        /// <param name="writer">Text writer to render view to.</param>
        /// <param name="model">The model to pass to the view renderer.</param>
        /// <param name="controllerContext">Active controller context.</param>
        public static void RenderPartialView(string viewPath, TextWriter writer, object model = null, ControllerContext controllerContext = null)
        {
            var renderer = new MvcViewEngine(controllerContext);
            renderer.RenderPartialView(viewPath, model, writer);
        }

        /// <summary>
        /// Renders an HtmlHelper delegate to a string. This method creates a temporary view context
        /// and captures the output of the HtmlHelper function.
        /// </summary>
        /// <param name="htmlHelperFunc">A function that takes an <see cref="HtmlHelper"/> and returns <see cref="MvcHtmlString"/> or <see cref="IHtmlString"/>.</param>
        /// <param name="model">The model to attach to the view data.</param>
        /// <param name="controllerContext">Active controller context (optional).</param>
        /// <returns>String representation of the rendered HTML helper output.</returns>
        public static string RenderHtmlHelperToString(Func<HtmlHelper, IHtmlString> htmlHelperFunc, object model = null, ControllerContext controllerContext = null)
        {
            if (htmlHelperFunc == null)
            {
                throw new ArgumentNullException(nameof(htmlHelperFunc));
            }

            var renderer = new MvcViewEngine(controllerContext);
            return renderer.RenderHtmlHelperToStringInternal(htmlHelperFunc, model);
        }

        /// <summary>
        /// Renders an HtmlHelper delegate to a string with error handling. This method creates a temporary view context
        /// and captures the output of the HtmlHelper function.
        /// </summary>
        /// <param name="htmlHelperFunc">A function that takes an <see cref="HtmlHelper"/> and returns <see cref="MvcHtmlString"/> or <see cref="IHtmlString"/>.</param>
        /// <param name="model">The model to attach to the view data.</param>
        /// <param name="controllerContext">Active controller context (optional).</param>
        /// <param name="errorMessage">Output parameter that captures any error message instead of throwing.</param>
        /// <returns>String representation of the rendered HTML helper output or <c>null</c> on error.</returns>
        public static string RenderHtmlHelperToString(
            Func<HtmlHelper, IHtmlString> htmlHelperFunc,
            object model,
            ControllerContext controllerContext,
            out string errorMessage)
        {
            errorMessage = null;
            try
            {
                return RenderHtmlHelperToString(htmlHelperFunc, model, controllerContext);
            }
            catch (Exception ex)
            {
                errorMessage = ex.GetBaseException().Message;
                return null;
            }
        }

        /// <summary>
        /// Creates an instance of an MVC controller from scratch
        /// when no existing <see cref="ControllerContext"/> is present.
        /// </summary>
        /// <typeparam name="T">Type of the controller to create.</typeparam>
        /// <param name="routeData">Optional route data used to initialize the controller context.</param>
        /// <param name="parameters">Optional constructor parameters for the controller.</param>
        /// <returns>Controller for <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current"/> is not available.</exception>
        public static T CreateController<T>(RouteData routeData = null, params object[] parameters)
                    where T : Controller, new()
        {
            // create a disconnected controller instance
            T controller = (T)Activator.CreateInstance(typeof(T), parameters);

            // get context wrapper from HttpContext if available
            HttpContextBase wrapper = null;
            if (HttpContext.Current != null)
            {
                wrapper = new HttpContextWrapper(HttpContext.Current);
            }
            else
            {
                throw new InvalidOperationException(
                    "Can't create Controller Context if no active HttpContext instance is available.");
            }

            if (routeData == null)
            {
                routeData = new RouteData();
            }

            // add the controller routing if not existing
            if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
            {
                routeData.Values.Add(
                    "controller",
                    controller.GetType().Name
                        .ToLower()
                        .Replace("controller", string.Empty));
            }

            controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
            return controller;
        }

        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by
        /// name, or as fully qualified ~/ path including extension.
        /// </param>
        /// <param name="model">The model to pass to the view renderer.</param>
        /// <returns>String of the rendered view or <c>null</c> on error.</returns>
        public string RenderPartialViewToString(string viewPath, object model = null)
        {
            return this.RenderViewToStringInternal(viewPath, model, true);
        }

        /// <summary>
        /// Renders a partial MVC view to given Writer. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by
        /// name, or as fully qualified ~/ path including extension.
        /// </param>
        /// <param name="model">The model to pass to the view renderer.</param>
        /// <param name="writer">Writer to render the view to.</param>
        public void RenderPartialView(string viewPath, object model, TextWriter writer)
        {
            this.RenderViewToWriterInternal(viewPath, writer, model, true);
        }

        /// <summary>
        /// Renders a full MVC view to a string. Will render with the full MVC
        /// view engine including running _ViewStart and merging into _Layout.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by
        /// name, or as fully qualified ~/ path including extension.
        /// </param>
        /// <param name="model">The model to render the view with.</param>
        /// <returns>String of the rendered view or <c>null</c> on error.</returns>
        public string RenderViewToString(string viewPath, object model = null)
        {
            return this.RenderViewToStringInternal(viewPath, model, false);
        }

        /// <summary>
        /// Renders a full MVC view to a writer. Will render with the full MVC
        /// view engine including running _ViewStart and merging into _Layout.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by
        /// name, or as fully qualified ~/ path including extension.
        /// </param>
        /// <param name="model">The model to render the view with.</param>
        /// <param name="writer">Writer to render the view to.</param>
        public void RenderView(string viewPath, object model, TextWriter writer)
        {
            this.RenderViewToWriterInternal(viewPath, writer, model, false);
        }

        /// <summary>
        /// Internal method that handles rendering of either partial or full views.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by
        /// name, or as fully qualified ~/ path including extension.
        /// </param>
        /// <param name="writer">Text writer to render view to.</param>
        /// <param name="model">Model to render the view with.</param>
        /// <param name="partial">Determines whether to render a full or partial view.</param>
        protected void RenderViewToWriterInternal(string viewPath, TextWriter writer, object model = null, bool partial = false)
        {
            // First find the ViewEngine for this view.
            ViewEngineResult viewEngineResult = null;
            if (partial)
            {
                viewEngineResult = ViewEngines.Engines.FindPartialView(this.Context, viewPath);
            }
            else
            {
                viewEngineResult = ViewEngines.Engines.FindView(this.Context, viewPath, null);
            }

            if (viewEngineResult == null)
            {
                throw new FileNotFoundException();
            }

            // Get the view and attach the model to view data.
            var view = viewEngineResult.View;
            this.Context.Controller.ViewData.Model = model;

            var ctx = new ViewContext(
                this.Context,
                view,
                this.Context.Controller.ViewData,
                this.Context.Controller.TempData,
                writer);
            view.Render(ctx, writer);
        }

        /// <summary>
        /// Internal method that handles rendering of either partial or full views.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by
        /// name, or as fully qualified ~/ path including extension.
        /// </param>
        /// <param name="model">Model to render the view with.</param>
        /// <param name="partial">Determines whether to render a full or partial view.</param>
        /// <returns>String of the rendered view.</returns>
        private string RenderViewToStringInternal(string viewPath, object model, bool partial = false)
        {
            // First find the ViewEngine for this view.
            ViewEngineResult viewEngineResult = null;
            if (partial)
            {
                viewEngineResult = ViewEngines.Engines.FindPartialView(this.Context, viewPath);
            }
            else
            {
                viewEngineResult = ViewEngines.Engines.FindView(this.Context, viewPath, null);
            }

            if (viewEngineResult == null || viewEngineResult.View == null)
            {
                throw new FileNotFoundException("ViewCouldNotBeFound");
            }

            // Get the view and attach the model to view data.
            var view = viewEngineResult.View;
            this.Context.Controller.ViewData.Model = model;

            string result;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(
                    this.Context,
                    view,
                    this.Context.Controller.ViewData,
                    this.Context.Controller.TempData,
                    sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }

        /// <summary>
        /// Internal method that handles rendering of an <see cref="HtmlHelper"/> delegate to a string.
        /// </summary>
        /// <param name="htmlHelperFunc">A function that takes an <see cref="HtmlHelper"/> and returns <see cref="MvcHtmlString"/> or <see cref="IHtmlString"/>.</param>
        /// <param name="model">Model to attach to the view data.</param>
        /// <returns>String representation of the rendered HTML helper output.</returns>
        private string RenderHtmlHelperToStringInternal(Func<HtmlHelper, IHtmlString> htmlHelperFunc, object model = null)
        {
            // Set the model to view data.
            this.Context.Controller.ViewData.Model = model;

            string result;

            using (var sw = new StringWriter())
            {
                // Create a view data dictionary for the HtmlHelper.
                var viewDataContainer = new ViewDataContainer(this.Context.Controller.ViewData);

                // Create the HtmlHelper with the proper context.
                var htmlHelper = new HtmlHelper(
                    new ViewContext(this.Context, new FakeView(), this.Context.Controller.ViewData, this.Context.Controller.TempData, sw),
                    viewDataContainer);

                // Execute the HtmlHelper function and capture the result.
                var htmlResult = htmlHelperFunc(htmlHelper);
                result = htmlResult?.ToString() ?? string.Empty;
            }

            return result;
        }
    }
}
