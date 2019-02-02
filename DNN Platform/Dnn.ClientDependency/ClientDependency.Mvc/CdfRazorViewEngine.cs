using System.Web.Mvc;

namespace ClientDependency.Core.Mvc
{
    public class CdfRazorViewEngine : RazorViewEngine
    {
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            return new CdfRazorView(controllerContext, partialPath, null, false, FileExtensions, ViewPageActivator);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            return new CdfRazorView(controllerContext, viewPath, masterPath, true, FileExtensions, ViewPageActivator);
        }
    }
}