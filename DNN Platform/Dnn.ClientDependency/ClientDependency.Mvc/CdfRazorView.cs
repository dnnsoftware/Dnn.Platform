using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace ClientDependency.Core.Mvc
{
    /// <summary>
    /// A razor view that ensures that CDF scripts/styles are rendered correctly
    /// </summary>
    public class CdfRazorView : RazorView
    {
        public CdfRazorView(ControllerContext controllerContext, string viewPath, string layoutPath, bool runViewStartPages, IEnumerable<string> viewStartFileExtensions)
            : base(controllerContext, viewPath, layoutPath, runViewStartPages, viewStartFileExtensions)
        {
        }

        public CdfRazorView(ControllerContext controllerContext, string viewPath, string layoutPath, bool runViewStartPages, IEnumerable<string> viewStartFileExtensions, IViewPageActivator viewPageActivator)
            : base(controllerContext, viewPath, layoutPath, runViewStartPages, viewStartFileExtensions, viewPageActivator)
        {
        }

        protected override void RenderView(ViewContext viewContext, TextWriter writer, object instance)
        {
            using (var replacements = new PlaceholderReplacer(writer, viewContext.HttpContext))
            {
                base.RenderView(viewContext, replacements.Writer, instance);
            }
        }
    }
}
