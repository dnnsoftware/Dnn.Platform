// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation;

using System.Web;

using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Users;
using Serilog.Core;
using Serilog.Events;

/// <summary>
/// A Serilog log event enricher that adds DNN-specific properties to log events.
/// </summary>
public class DnnEnricher : ILogEventEnricher
{
    /// <summary>
    /// Enriches the log event with DNN-specific properties including UserId and PortalId.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">The factory used to create log event properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var portalId = -1;
        if (HttpContext.Current != null)
        {
            object contextObject = HttpContext.Current.Items["PortalSettings"];
            if (contextObject != null)
            {
                var portalSettings = (IPortalSettings)contextObject;
                portalId = portalSettings.PortalId;
            }
        }

        var userId = -1;
        if (HttpContext.Current != null)
        {
            object contextObject = HttpContext.Current.Items["UserInfo"];
            if (contextObject != null)
            {
                var userInfo = (IUserInfo)contextObject;
                userId = userInfo.UserID;
            }
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PortalId", portalId));
    }
}
