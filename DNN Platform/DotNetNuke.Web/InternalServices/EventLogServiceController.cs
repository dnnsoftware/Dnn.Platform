// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A web API which gets event log details.</summary>
[DnnAuthorize]
public class EventLogServiceController : DnnApiController
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventLogServiceController));
    private readonly IEventLogService eventLogService;

    /// <summary>Initializes a new instance of the <see cref="EventLogServiceController"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogService. Scheduled removal in v12.0.0.")]
    public EventLogServiceController()
        : this(null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="EventLogServiceController"/> class.</summary>
    /// <param name="eventLogService">The event log service.</param>
    public EventLogServiceController(IEventLogService eventLogService)
    {
        this.eventLogService = eventLogService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogService>();
    }

    /// <summary>Gets event log details.</summary>
    /// <param name="guid">The event log GUID.</param>
    /// <returns>A response with an object with <c>Title</c> and <c>Content</c> fields.</returns>
    [HttpGet]
    [DnnAuthorize(StaticRoles = "Administrators")]
    [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Breaking change")]
    public HttpResponseMessage GetLogDetails(string guid)
    {
        if (string.IsNullOrEmpty(guid) || !Guid.TryParse(guid, out _))
        {
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        try
        {
            if (this.eventLogService.GetLog(guid) is not ILogInfo logInfo)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                Title = Localization.GetSafeJSString("CriticalError.Error", Localization.SharedResourceFile),
                Content = GetPropertiesText(logInfo),
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }

    private static string GetPropertiesText(ILogInfo logInfo)
    {
        var str = new StringBuilder();
        foreach (var ldi in logInfo.LogProperties)
        {
            // display the values in the Panel child controls.
            if (ldi.PropertyName == "Message")
            {
                str.Append($"<p><strong>{ldi.PropertyName}</strong>:</br><pre>{HttpUtility.HtmlEncode(ldi.PropertyValue)}</pre></p>");
            }
            else
            {
                str.Append($"<p><strong>{ldi.PropertyName}</strong>:{HttpUtility.HtmlEncode(ldi.PropertyValue)}</p>");
            }
        }

        str.Append($"<p><b>Server Name</b>: {HttpUtility.HtmlEncode(logInfo.LogServerName)}</p>");
        return str.ToString();
    }
}
