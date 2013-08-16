using System;
using System.Web.UI;

namespace DotNetNuke.Framework
{
    internal interface IServiceFrameworkInternals
    {
        bool IsAjaxAntiForgerySupportRequired { get; }

        void RegisterAjaxAntiForgery(Page page);

        bool IsAjaxScriptSupportRequired { get; }

        void RegisterAjaxScript(Page page);
    }
}