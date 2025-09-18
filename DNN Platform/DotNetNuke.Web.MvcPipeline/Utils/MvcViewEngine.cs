using System;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;


namespace DotNetNuke.Web.MvcPipeline.Utils
{
    /// <summary>
    /// Class that renders MVC views to a string using the
    /// standard MVC View Engine to render the view. 
    /// 
    /// Requires that ASP.NET HttpContext is present to
    /// work, but works outside of the context of MVC
    /// credits to https://weblog.west-wind.com/posts/2012/may/30/rendering-aspnet-mvc-views-to-string
    /// </summary>
    public class MvcViewEngine
    {
        /// <summary>
        /// Required Controller Context
        /// </summary>
        protected ControllerContext Context { get; set; }

        /// <summary>
        /// Initializes the ViewRenderer with a Context.
        /// </summary>
        /// <param name="controllerContext">
        /// If you are running within the context of an ASP.NET MVC request pass in
        /// the controller's context. 
        /// Only leave out the context if no context is otherwise available.
        /// </param>
        public MvcViewEngine(ControllerContext controllerContext = null)
        {
            // Create a known controller from HttpContext if no context is passed
            if (controllerContext == null)
            {
                if (HttpContext.Current != null)
                    controllerContext = CreateController<EmptyController>().ControllerContext;
                else
                    throw new InvalidOperationException(
                        "ViewRenderer must run in the context of an ASP.NET " +
                        "Application and requires HttpContext.Current to be present.");
            }
            Context = controllerContext;
        }

        /// <summary>
        /// Renders a full MVC view to a string. Will render with the full MVC
        /// View engine including running _ViewStart and merging into _Layout        
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to render the view with</param>
        /// <returns>String of the rendered view or null on error</returns>
        public string RenderViewToString(string viewPath, object model = null)
        {
            return RenderViewToStringInternal(viewPath, model, false);
        }

        /// <summary>
        /// Renders a full MVC view to a writer. Will render with the full MVC
        /// View engine including running _ViewStart and merging into _Layout        
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to render the view with</param>
        /// <returns>String of the rendered view or null on error</returns>
        public void RenderView(string viewPath, object model, TextWriter writer)
        {
            RenderViewToWriterInternal(viewPath, writer, model, false);
        }


        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <returns>String of the rendered view or null on error</returns>
        public string RenderPartialViewToString(string viewPath, object model = null)
        {
            return RenderViewToStringInternal(viewPath, model, true);
        }

        /// <summary>
        /// Renders a partial MVC view to given Writer. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="writer">Writer to render the view to</param>
        public void RenderPartialView(string viewPath, object model, TextWriter writer)
        {
            RenderViewToWriterInternal(viewPath, writer, model, true);
        }

        /// <summary>
        /// Renders a full MVC view to a writer. Will render with the full MVC
        /// View engine including running _ViewStart and merging into _Layout
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="controllerContext">Active Controller context</param>
        /// <returns>String of the rendered view or null on error</returns>
        public static string RenderView(string viewPath, object model = null,
                                        ControllerContext controllerContext = null)
        {
            MvcViewEngine renderer = new MvcViewEngine(controllerContext);
            return renderer.RenderViewToString(viewPath, model);
        }

        /// <summary>
        /// Renders a full MVC view to a writer. Will render with the full MVC
        /// View engine including running _ViewStart and merging into _Layout
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="writer">Writer to render the view to</param>
        /// <param name="controllerContext">Active Controller context</param>
        /// <returns>String of the rendered view or null on error</returns>
        public static void RenderView(string viewPath, TextWriter writer, object model,
                                        ControllerContext controllerContext)
        {
            MvcViewEngine renderer = new MvcViewEngine(controllerContext);
            renderer.RenderView(viewPath, model, writer);
        }

        /// <summary>
        /// Renders a full MVC view to a writer. Will render with the full MVC
        /// View engine including running _ViewStart and merging into _Layout
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="controllerContext">Active Controller context</param>
        /// <param name="errorMessage">optional out parameter that captures an error message instead of throwing</param>
        /// <returns>String of the rendered view or null on error</returns>
        public static string RenderView(string viewPath, object model,
                                        ControllerContext controllerContext,
                                        out string errorMessage)
        {
            errorMessage = null;
            try
            {
                MvcViewEngine renderer = new MvcViewEngine(controllerContext);
                return renderer.RenderViewToString(viewPath, model);
            }
            catch (Exception ex)
            {
                errorMessage = ex.GetBaseException().Message;
            }
            return null;
        }

        /// <summary>
        /// Renders a full MVC view to a writer. Will render with the full MVC
        /// View engine including running _ViewStart and merging into _Layout
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="controllerContext">Active Controller context</param>
        /// <param name="writer">Writer to render the view to</param>
        /// <param name="errorMessage">optional out parameter that captures an error message instead of throwing</param>
        /// <returns>String of the rendered view or null on error</returns>
        public static void RenderView(string viewPath, object model, TextWriter writer,
                                        ControllerContext controllerContext,
                                        out string errorMessage)
        {
            errorMessage = null;
            try
            {
                MvcViewEngine renderer = new MvcViewEngine(controllerContext);
                renderer.RenderView(viewPath, model, writer);
            }
            catch (Exception ex)
            {
                errorMessage = ex.GetBaseException().Message;
            }
        }


        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="controllerContext">Active controller context</param>
        /// <returns>String of the rendered view or null on error</returns>
        public static string RenderPartialView(string viewPath, object model = null,
                                                ControllerContext controllerContext = null)
        {
            MvcViewEngine renderer = new MvcViewEngine(controllerContext);
            return renderer.RenderPartialViewToString(viewPath, model);
        }

        /// <summary>
        /// Renders a partial MVC view to string. Use this method to render
        /// a partial view that doesn't merge with _Layout and doesn't fire
        /// _ViewStart.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">The model to pass to the viewRenderer</param>
        /// <param name="controllerContext">Active controller context</param>
        /// <param name="writer">Text writer to render view to</param>
        /// <param name="errorMessage">optional output parameter to receive an error message on failure</param>
        public static void RenderPartialView(string viewPath, TextWriter writer, object model = null,
                                                ControllerContext controllerContext = null)
        {
            MvcViewEngine renderer = new MvcViewEngine(controllerContext);
            renderer.RenderPartialView(viewPath, model, writer);
        }

        /// <summary>
        /// Renders an HtmlHelper delegate to a string. This method creates a temporary view context
        /// and captures the output of the HtmlHelper function.
        /// </summary>
        /// <param name="htmlHelperFunc">A function that takes an HtmlHelper and returns MvcHtmlString or IHtmlString</param>
        /// <param name="model">The model to attach to the view data</param>
        /// <param name="controllerContext">Active controller context (optional)</param>
        /// <returns>String representation of the rendered HTML helper output</returns>
        public static string RenderHtmlHelperToString(Func<HtmlHelper, IHtmlString> htmlHelperFunc, object model = null, ControllerContext controllerContext = null)
        {
            if (htmlHelperFunc == null)
                throw new ArgumentNullException(nameof(htmlHelperFunc));

            MvcViewEngine renderer = new MvcViewEngine(controllerContext);
            return renderer.RenderHtmlHelperToStringInternal(htmlHelperFunc, model);
        }

        /// <summary>
        /// Renders an HtmlHelper delegate to a string with error handling. This method creates a temporary view context
        /// and captures the output of the HtmlHelper function.
        /// </summary>
        /// <param name="htmlHelperFunc">A function that takes an HtmlHelper and returns MvcHtmlString or IHtmlString</param>
        /// <param name="model">The model to attach to the view data</param>
        /// <param name="controllerContext">Active controller context (optional)</param>
        /// <param name="errorMessage">Output parameter that captures any error message instead of throwing</param>
        /// <returns>String representation of the rendered HTML helper output or null on error</returns>
        public static string RenderHtmlHelperToString(Func<HtmlHelper, IHtmlString> htmlHelperFunc, object model, ControllerContext controllerContext, out string errorMessage)
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
        /// Internal method that handles rendering of either partial or 
        /// or full views.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">Model to render the view with</param>
        /// <param name="partial">Determines whether to render a full or partial view</param>
        /// <param name="writer">Text writer to render view to</param>
        protected void RenderViewToWriterInternal(string viewPath, TextWriter writer, object model = null, bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException();

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            Context.Controller.ViewData.Model = model;

            var ctx = new ViewContext(Context, view,
                                        Context.Controller.ViewData,
                                        Context.Controller.TempData,
                                        writer);
            view.Render(ctx, writer);
        }

        /// <summary>
        /// Internal method that handles rendering of either partial or 
        /// or full views.
        /// </summary>
        /// <param name="viewPath">
        /// The path to the view to render. Either in same controller, shared by 
        /// name or as fully qualified ~/ path including extension
        /// </param>
        /// <param name="model">Model to render the view with</param>
        /// <param name="partial">Determines whether to render a full or partial view</param>
        /// <returns>String of the rendered view</returns>
        private string RenderViewToStringInternal(string viewPath, object model,
                                                    bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;
            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

            if (viewEngineResult == null || viewEngineResult.View == null)
                throw new FileNotFoundException("ViewCouldNotBeFound");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            Context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(Context, view,
                                            Context.Controller.ViewData,
                                            Context.Controller.TempData,
                                            sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }

        /// <summary>
        /// Internal method that handles rendering of HtmlHelper delegate to a string.
        /// </summary>
        /// <param name="htmlHelperFunc">A function that takes an HtmlHelper and returns MvcHtmlString or IHtmlString</param>
        /// <param name="model">Model to attach to the view data</param>
        /// <returns>String representation of the rendered HTML helper output</returns>
        private string RenderHtmlHelperToStringInternal(Func<HtmlHelper, IHtmlString> htmlHelperFunc, object model = null)
        {
            // Set the model to view data
            Context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                // Create a view data dictionary for the HtmlHelper
                var viewDataContainer = new ViewDataContainer(Context.Controller.ViewData);
                
                // Create the HtmlHelper with the proper context
                var htmlHelper = new HtmlHelper(
                    new ViewContext(Context, new FakeView(), Context.Controller.ViewData, Context.Controller.TempData, sw),
                    viewDataContainer);

                // Execute the HtmlHelper function and capture the result
                var htmlResult = htmlHelperFunc(htmlHelper);
                result = htmlResult?.ToString() ?? string.Empty;
            }

            return result;
        }


        /// <summary>
        /// Creates an instance of an MVC controller from scratch 
        /// when no existing ControllerContext is present       
        /// </summary>
        /// <typeparam name="T">Type of the controller to create</typeparam>
        /// <returns>Controller for T</returns>
        /// <exception cref="InvalidOperationException">thrown if HttpContext not available</exception>
        public static T CreateController<T>(RouteData routeData = null, params object[] parameters)
                    where T : Controller, new()
        {
            // create a disconnected controller instance
            T controller = (T) Activator.CreateInstance(typeof (T), parameters);

            // get context wrapper from HttpContext if available
            HttpContextBase wrapper = null;
            if (HttpContext.Current != null)
                wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
            else
                throw new InvalidOperationException(
                    "Can't create Controller Context if no active HttpContext instance is available.");

            if (routeData == null)
                routeData = new RouteData();

            // add the controller routing if not existing
            if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
                routeData.Values.Add("controller", controller.GetType().Name
                                                            .ToLower()
                                                            .Replace("controller", ""));

            controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
            return controller;
        }

    }

    /// <summary>
    /// Empty MVC Controller instance used to 
    /// instantiate and provide a new ControllerContext
    /// for the ViewRenderer
    /// </summary>
    public class EmptyController : Controller
    {
    }

    /// <summary>
    /// Simple ViewDataContainer implementation for HtmlHelper usage
    /// </summary>
    internal class ViewDataContainer : IViewDataContainer
    {
        public ViewDataDictionary ViewData { get; set; }

        public ViewDataContainer(ViewDataDictionary viewData)
        {
            ViewData = viewData;
        }
    }

    /// <summary>
    /// Fake view implementation for HtmlHelper contexts that don't require actual view rendering
    /// </summary>
    internal class FakeView : IView
    {
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            // No-op implementation since we're only using this for HtmlHelper context
        }
    }
}
