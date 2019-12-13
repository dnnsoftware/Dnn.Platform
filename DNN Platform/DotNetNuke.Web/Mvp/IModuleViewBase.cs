#region Usings

using System;

using DotNetNuke.UI.Skins.Controls;

using WebFormsMvp;

#endregion

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public interface IModuleViewBase : IView
    {
        bool AutoDataBind { get; set; }

        event EventHandler Initialize;

        void ProcessModuleLoadException(Exception ex);

        void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType);
    }
}
