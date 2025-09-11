namespace DotNetNuke.Web.MvcPipeline.UI.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Web.UI;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;

    public class MvcUtils
    {
        public static string GetControlViewName(ModuleInfo module)
        {
            return GetControlViewName(module, Path.GetFileNameWithoutExtension(module.ModuleControl.ControlSrc));
        }

        public static string GetControlViewName(ModuleInfo module, string viewName)
        {
            return "~/" + Path.GetDirectoryName(module.ModuleControl.ControlSrc) + "/Views/" + viewName + ".cshtml";
        }

        public static string GetControlControllerName(ModuleInfo module)
        {
            return GetControlControllerName(module.ModuleControl.ControlSrc);
        }

        public static string GetControlControllerName(string controlSrc)
        {
            if (controlSrc.StartsWith("DesktopModules"))
            {
                // return controlSrc.Replace("DesktopModules/", string.Empty).Replace("/", string.Empty).Replace(".ascx", string.Empty) + "View";
                return Path.GetFileNameWithoutExtension(controlSrc) + "View";
            }
            else
            {
                return Path.GetFileNameWithoutExtension(controlSrc) + "View";
            }
        }

        static private IDictionary<string, string> _moduleClasses = new Dictionary<string, string>() {
            { "ModuleActions", "DotNetNuke.Web.MvcWebsite.Controls.ModuleActionsControl, DotNetNuke.Web.MvcWebsite" },
            { "Admin/Portal/Terms.ascx", "DotNetNuke.Web.MvcWebsite.Controls.TermsControl, DotNetNuke.Web.MvcWebsite" },
            { "Admin/Portal/Privacy.ascx", "DotNetNuke.Web.MvcWebsite.Controls.PrivacyControl, DotNetNuke.Web.MvcWebsite" }
        };

        public static IMvcModuleControl GetModuleControl(ModuleInfo module)
        {
            return GetModuleControl(module, module.ModuleControl.ControlSrc);
        }

        public static IMvcModuleControl GetModuleControl(ModuleInfo module, string controlSrc)
        {
            IMvcModuleControl control;
            if (_moduleClasses.ContainsKey(controlSrc))
            {
                var controlClass = _moduleClasses[controlSrc];
                try
                {
                    var controller = Reflection.CreateObject(Globals.DependencyProvider, controlClass, controlClass);
                    control = controller as IMvcModuleControl;
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not create instance of " + controlClass, ex);
                }
            }
            else
            {
                if (module.DesktopModule == null)
                {
                    throw new Exception("No DesktopModule is not defined for the module " + module.ModuleTitle);
                }

                if (controlSrc.EndsWith(".mvc", System.StringComparison.OrdinalIgnoreCase))
                {
                    control = new MvcModuleControl();
                }
                else if (controlSrc.EndsWith(".html", System.StringComparison.OrdinalIgnoreCase))
                {
                    control = new SpaModuleControl();
                }
                else
                {
                    if (string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass))
                    {
                        throw new Exception("The BusinessControllerClass is not defined for the module " + module.ModuleDefinition.FriendlyName);
                    }
                    var businessController = Reflection.CreateType(module.DesktopModule.BusinessControllerClass);
                    var controlClass = businessController.Namespace + "." + System.IO.Path.GetFileNameWithoutExtension(controlSrc) + "Control," + businessController.Assembly;
                    try
                    {
                        var controller = Reflection.CreateObject(Globals.DependencyProvider, controlClass, controlClass);
                        control = controller as IMvcModuleControl;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Could not create instance of " + controlClass, ex);
                    }
                }
            }
            control.ModuleContext.Configuration = module;
            return control;
        }
    }
}
