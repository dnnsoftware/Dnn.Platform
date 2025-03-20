namespace DotNetNuke.Web.MvcPipeline.UI.Utilities
{
    using System.IO;

    using DotNetNuke.Entities.Modules;

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
                return controlSrc.Replace("DesktopModules/", string.Empty).Replace("/", string.Empty).Replace(".ascx", string.Empty) + "View";
            }
            else
            {
                return Path.GetFileNameWithoutExtension(controlSrc).Replace("/", string.Empty).Replace(".ascx", string.Empty) + "View";
            }
        }
    }
}
