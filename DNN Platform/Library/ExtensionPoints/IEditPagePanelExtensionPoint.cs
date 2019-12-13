using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IEditPagePanelExtensionPoint: IUserControlExtensionPoint
    {
        string EditPagePanelId { get; }
        string CssClass { get; }        
    }
}
