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
        public override string ControlName
        {
            get
            {
                return "TestModuleControl";
            }
        }

        /// <summary>Gets or Sets the Path for this control (used primarily for UserControls).</summary>
        /// <returns>A String.</returns>
        public override string ControlPath
        {
            get
            {

                return "DesktopModules/mvcpl/mvcpl1";
            }
        }

        public override IRazorModuleResult Invoke()
        {
            return View("~/Views/Default/Terms.cshtml", "Hello from TestModuleControl");
        }
    }
}
