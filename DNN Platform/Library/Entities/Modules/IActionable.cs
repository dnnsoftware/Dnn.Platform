#region Usings

using DotNetNuke.Entities.Modules.Actions;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public interface IActionable
    {
        ModuleActionCollection ModuleActions { get; }
    }
}
