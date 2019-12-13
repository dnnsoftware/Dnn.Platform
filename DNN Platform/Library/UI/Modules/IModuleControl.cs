#region Usings

using System.Web.UI;

#endregion

namespace DotNetNuke.UI.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class	 : IModuleControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// IModuleControl provides a common Interface for Module Controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface IModuleControl
    {
        Control Control { get; }
        string ControlPath { get; }
        string ControlName { get; }
        string LocalResourceFile { get; set; }
        ModuleInstanceContext ModuleContext { get; }
    }
}
