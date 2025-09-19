using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Demo
{
    public class DemoModuleControl : RazorModuleControlBase
    {
        public override string ControlName => "DemoModuleControl";

        public override string ControlPath => "DesktopModules/Demo";

        public override IRazorModuleResult Invoke()
        {
            if (Request.QueryString["view"] == "Terms")
            {
                return Terms();
            }
            else if (Request.QueryString["view"] == "Privacy")
            {
                return Privacy();
            }
            else
            {
                return View("~/admin/Portal/Views/Terms.cshtml", "Hello from DemoModuleControl - Default view");
            }
        }

        private IRazorModuleResult Privacy()
        {
            return View("~/admin/Portal/Views/Privacy.cshtml", "Hello from DemoModuleControl - Privacy view");
        }

        private IRazorModuleResult Terms()
        {
            return View("~/Views/Default/Terms.cshtml", "Hello from DemoModuleControl - Terms view");
        }
    }
}
