#region Usings

using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.UI.Containers
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Containers
    /// Class	 : IActionControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// IActionControl provides a common INterface for Action Controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface IActionControl
    {
        ActionManager ActionManager { get; }
        IModuleControl ModuleControl { get; set; }
        event ActionEventHandler Action;
    }
}
