namespace DotNetNuke.ExtensionPoints
{
    public interface IContextMenuItemExtensionPoint : IExtensionPoint
    {
        string CtxMenuItemId { get; }

        string CssClass { get; }

        string Action { get; }

        string AltText { get; }

        bool ShowText { get; }

        bool ShowIcon { get; }
    }
}
