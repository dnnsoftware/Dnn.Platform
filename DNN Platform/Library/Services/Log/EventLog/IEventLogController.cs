// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IEventLogController : ILogController
    {
        void AddLog(string propertyName, string propertyValue, EventLogController.EventLogType logType);

        [Obsolete("Deprecated in DNN 9.7.  It has been replaced by the overload taking IPortalSettings. Scheduled removal in v11.0.0.")]
        void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID, EventLogController.EventLogType logType);

        [Obsolete("Deprecated in DNN 9.7.  It has been replaced by the overload taking IPortalSettings. Scheduled removal in v11.0.0.")]
        void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID, string logType);

        void AddLog(string propertyName, string propertyValue, IPortalSettings portalSettings, int userID, EventLogController.EventLogType logType);

        void AddLog(string propertyName, string propertyValue, IPortalSettings portalSettings, int userID, string logType);

        void AddLog(PortalSettings portalSettings, int userID, EventLogController.EventLogType logType);

        [Obsolete("Deprecated in DNN 9.7.  It has been replaced by the overload taking IPortalSettings. Scheduled removal in v11.0.0.")]
        void AddLog(LogProperties properties, PortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering);

        [Obsolete("Deprecated in DNN 9.7.  It has been replaced by the overload taking IPortalSettings. Scheduled removal in v11.0.0.")]
        void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName, EventLogController.EventLogType logType);

        [Obsolete("Deprecated in DNN 9.7.  It has been replaced by the overload taking IPortalSettings. Scheduled removal in v11.0.0.")]
        void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName, string logType);

        void AddLog(LogProperties properties, IPortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering);

        void AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, EventLogController.EventLogType logType);

        void AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, string logType);
    }
}
