using DotNetNuke.UI.Modules;

namespace DotNetNuke.ExtensionPoints
{
    public interface IToolBarButtonExtensionPoint : IExtensionPoint
    {
        string ButtonId { get; }

        string CssClass { get; }

        string Action { get; }

        string AltText { get; }

        bool ShowText { get; }

        bool ShowIcon { get; }

        bool Enabled { get; }

        ModuleInstanceContext ModuleContext { get; set; }
    }
}
