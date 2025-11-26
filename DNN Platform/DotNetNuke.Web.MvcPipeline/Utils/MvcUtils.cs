namespace DotNetNuke.Web.MvcPipeline.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Web.UI;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using Microsoft.Extensions.DependencyInjection;

    public class MvcUtils
    {
        public static IMvcModuleControl CreateModuleControl(ModuleInfo module)
        {
            return GetModuleControl(module, module.ModuleControl.ControlSrc);
        }

        public static IMvcModuleControl GetModuleControl(ModuleInfo module, string controlSrc)
        {
            IMvcModuleControl control;
            if (!string.IsNullOrEmpty(module.ModuleControl.MvcControlClass))
            {
                var controlClass = module.ModuleControl.MvcControlClass;
                try
                {
                    var obj = Reflection.CreateObject(Globals.GetCurrentServiceProvider(), controlClass, controlClass);
                    if (obj is IMvcModuleControl)
                    {
                        control = obj as IMvcModuleControl;
                    }
                    else
                    {
                        throw new Exception("Mvc Control needs to implement IMvcModuleControl : " + controlClass);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not create instance of " + controlClass, ex);
                }
            }
            //else if (controlSrc.EndsWith(".mvc", System.StringComparison.OrdinalIgnoreCase))
            //{
            //    control = new MvcModuleControl();
            //}
            else if (controlSrc.EndsWith(".html", System.StringComparison.OrdinalIgnoreCase))
            {
                control = new SpaModuleControl();
            }
            else
            {
                throw new Exception("The module control dous not support the MVC pipeline : " + module.ModuleTitle + " " + module.ModuleControl.ControlTitle);
            }

            control.ModuleContext.Configuration = module;
            return control;
        }
    }
}
