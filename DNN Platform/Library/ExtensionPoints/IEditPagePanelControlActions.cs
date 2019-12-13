using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IEditPagePanelControlActions
    {
        void SaveAction(int portalId, int tabId, int moduleId);
        void CancelAction(int portalId, int tabId, int moduleId);
        void BindAction(int portalId, int tabId, int moduleId);
    }
}
