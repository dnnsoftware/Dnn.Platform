using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Test
{
    public class TestModuleControl : RazorModuleControlBase
    {
        public override string ControlName => "TestModuleControl";

        public override string ControlPath => "DesktopModules/mvcpl/mvcpl1";

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
                return View("~/admin/Portal/Views/Terms.cshtml", "Hello from TestModuleControl - Default view");
            }
        }

        private IRazorModuleResult Privacy()
        {
            return View("~/admin/Portal/Views/Privacy.cshtml", "Hello from TestModuleControl - Privacy view");
        }

        private IRazorModuleResult Terms()
        {
            return View("~/Views/Default/Terms.cshtml", "Hello from TestModuleControl - Terms view");
        }
    }
}
