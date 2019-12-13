using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IMenuButtonItemExtensionPoint : IExtensionPoint
    {
        string ItemId { get; }

        string Attributes { get; }

        string Type { get; }

        string CssClass { get; }

        string Action { get; }
    }
}
