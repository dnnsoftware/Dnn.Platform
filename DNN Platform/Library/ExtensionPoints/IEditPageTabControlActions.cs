using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IEditPageTabControlActions
    {
        void SaveAction(int portalId, int tabId, int moduleId);
        void CancelAction(int portalId, int tabId, int moduleId);
        void BindAction(int portalId, int tabId, int moduleId);
    }
}
