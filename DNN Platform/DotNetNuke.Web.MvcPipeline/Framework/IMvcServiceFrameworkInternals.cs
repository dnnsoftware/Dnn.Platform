namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System.Web.Mvc;

    internal interface IMvcServiceFrameworkInternals
    {
        bool IsAjaxAntiForgerySupportRequired { get; }

        bool IsAjaxScriptSupportRequired { get; }
        void RegisterAjaxAntiForgery(ControllerContext page);
        void RegisterAjaxScript(ControllerContext page);
    }
}
