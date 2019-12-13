namespace DotNetNuke.ExtensionPoints
{
    public interface IScriptItemExtensionPoint : IExtensionPoint
    {
        string ScriptName { get; }
    }
}
