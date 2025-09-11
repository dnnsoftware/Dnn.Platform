using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
using DotNetNuke.Web.MvcPipeline.Utils;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public class MvcModuleControlRenderer<T> where T : RazorModuleControlBase, new()
    {
        private readonly T moduleControl;

        public MvcModuleControlRenderer(Control control, ModuleInstanceContext moduleContext)
        {
            this.moduleControl = new T();
            moduleControl.ModuleContext = moduleContext;
            moduleControl.Control = control;
        }

        // Fix for CS0149: Replace the invalid constructor chaining syntax with a proper constructor call
        public MvcModuleControlRenderer(PortalModuleBase control)
            : this(control, control.ModuleContext)
        {
        }

        public string RenderToString()
        {
            var renderer = new ViewRenderer();
            return renderer.RenderViewToString(moduleControl.ViewName, moduleControl.ViewModel());
        }

        public T ModuleControl => this.moduleControl;

        public void RegisterResources(IResourcable resourcable)
        {
            if (resourcable.ModuleResources.StyleSheets != null)
            {
                foreach (var styleSheet in resourcable.ModuleResources.StyleSheets)
                {
                    ClientResourceManager.RegisterStyleSheet(ModuleControl.Control.Page, styleSheet.FilePath, styleSheet.Priority, styleSheet.HtmlAttributes);
                }
            }
            if (resourcable.ModuleResources.Scripts != null)
            {
                foreach (var javaScript in resourcable.ModuleResources.Scripts)
                {
                    ClientResourceManager.RegisterScript(ModuleControl.Control.Page, javaScript.FilePath, javaScript.Priority, javaScript.HtmlAttributes);
                }
            }
            if (resourcable.ModuleResources.Libraries != null)
            {
                foreach (var lib in resourcable.ModuleResources.Libraries)
                {
                    JavaScript.RequestRegistration(lib);
                }
            }
            if (resourcable.ModuleResources.AjaxScript)
            {
                ServicesFramework.Instance.RequestAjaxScriptSupport();
            }
            if (resourcable.ModuleResources.AjaxAntiForgery)
            {
                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            }
        }
    }
}
