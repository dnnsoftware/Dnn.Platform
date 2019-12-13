using System.Web;
using DotNetNuke.Web.Mvc.Framework.Modules;

namespace DotNetNuke.Web.Mvc.Routing
{
    public static class HttpContextExtensions
    {
        public const string ModuleRequestResultKey = "Dnn_ModuleRequestResult";

        public static ModuleRequestResult GetModuleRequestResult(this HttpContextBase context)
        {
            return context.Items[ModuleRequestResultKey] as ModuleRequestResult;
        }

        public static bool HasModuleRequestResult(this HttpContextBase context)
        {
            return (context.Items[ModuleRequestResultKey] as ModuleRequestResult) != null;
        }

        public static void SetModuleRequestResult(this HttpContextBase context, ModuleRequestResult moduleRequestResult)
        {
            context.Items[ModuleRequestResultKey] = moduleRequestResult;
        }
    }
}
