// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging;

using DotNetNuke.Abstractions.Portals;

/// <summary>
/// The event logger provides APIs for adding
/// new event logs.
/// </summary>
public interface IEventLogger
{
    /// <summary>Adds an Event Log.</summary>
    /// <param name="name">The log property name.</param>
    /// <param name="value">The log property value.</param>
    /// <param name="logType">The log type.</param>
    void AddLog(string name, string value, EventLogType logType);

    /// <summary>Adds an Event Log.</summary>
    /// <param name="name">The log property name.</param>
    /// <param name="value">The log property value.</param>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="userID">The user id.</param>
    /// <param name="logType">The log type.</param>
    void AddLog(string name, string value, IPortalSettings portalSettings, int userID, EventLogType logType);

    /// <summary>Adds an Event Log.</summary>
    /// <param name="name">The log property name.</param>
    /// <param name="value">The log property value.</param>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="userID">The user id.</param>
    /// <param name="logType">The log type.</param>
    void AddLog(string name, string value, IPortalSettings portalSettings, int userID, string logType);

    /// <summary>Adds an Event Log.</summary>
    /// <param name="properties">The properties of the log.</param>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="userID">The user id.</param>
    /// <param name="logTypeKey">The log type key.</param>
    /// <param name="bypassBuffering">The bypass buffering.</param>
    void AddLog(ILogProperties properties, IPortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering);

    /// <summary>Adds an Event Log.</summary>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="userID">The user id.</param>
    /// <param name="logType">The log type.</param>
    void AddLog(IPortalSettings portalSettings, int userID, EventLogType logType);

    /// <summary>Adds an Event Log.</summary>
    /// <param name="businessObject">The business object.</param>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="userID">The user id.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="logType">The log type.</param>
    void AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, EventLogType logType);

    /// <summary>Adds an Event Log.</summary>
    /// <param name="businessObject">The business object.</param>
    /// <param name="portalSettings">The portal settings.</param>
    /// <param name="userID">The user id.</param>
    /// <param name="userName">The user name.</param>
    /// <param name="logType">The log type.</param>
    void AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, string logType);

    /// <summary>Adds an Event Log.</summary>
    /// <param name="logInfo">The log info.</param>
    void AddLog(ILogInfo logInfo);
}
