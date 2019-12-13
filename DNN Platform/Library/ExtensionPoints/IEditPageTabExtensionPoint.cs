using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IEditPageTabExtensionPoint : IUserControlExtensionPoint
    {
        string EditPageTabId { get; }
        string CssClass { get; }
        string Permission { get; }
    }
}
