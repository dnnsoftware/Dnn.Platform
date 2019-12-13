namespace DotNetNuke.ExtensionPoints
{
    public interface IMenuItemExtensionPoint : IExtensionPoint
    {
        string Value { get; }

        string CssClass { get; }
    }
}
