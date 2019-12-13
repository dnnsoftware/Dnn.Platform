#region Usings

using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.UI.Skins
{
    public interface ISkinControl
    {
        IModuleControl ModuleControl { get; set; }
    }
}
