namespace DotNetNuke.ExtensionPoints
{
    public interface IExtensionPoint
    {
        string Text { get; }
        string Icon { get; }
        int Order { get; }
    }
}
