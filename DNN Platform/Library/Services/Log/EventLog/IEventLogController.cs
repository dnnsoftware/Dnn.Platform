using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Services.Log.EventLog
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IEventLogController
    {
        void AddLog(string propertyName, string propertyValue, EventLogController.EventLogType logType);

        void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID, EventLogController.EventLogType logType);

        void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID, string logType);

        void AddLog(PortalSettings portalSettings, int userID, EventLogController.EventLogType logType);

        void AddLog(LogProperties properties, PortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering);

        void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName, EventLogController.EventLogType logType);

        void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName, string logType);
    }
}