// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Instrumentation;

using System;

using Serilog.Events;

/// <summary>
/// Enriches log events with system information such as host name, app domain ID, and thread ID.
/// </summary>
public class DnnSystemInfoEnricher : Serilog.Core.ILogEventEnricher
{
    private readonly string hostName;
    private readonly int appDomainId;

    private LogEventProperty hostNameProperty;
    private LogEventProperty appDomainProperty;

    /// <summary>
    /// Initializes a new instance of the <see cref="DnnSystemInfoEnricher"/> class.
    /// </summary>
    public DnnSystemInfoEnricher()
    {
        this.appDomainId = AppDomain.CurrentDomain.Id;
        try
        {
            this.hostName = System.Net.Dns.GetHostName();
        }
        catch
        {
            this.hostName = Environment.MachineName;
        }
    }

    /// <summary>
    /// Enriches the log event with system information properties.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
    public void Enrich(LogEvent logEvent, Serilog.Core.ILogEventPropertyFactory propertyFactory)
    {
        this.hostNameProperty = this.hostNameProperty ?? propertyFactory.CreateProperty("HostName", this.hostName);
        this.appDomainProperty = this.appDomainProperty ?? propertyFactory.CreateProperty("AppDomain", this.appDomainId);

        logEvent.AddPropertyIfAbsent(this.hostNameProperty);
        logEvent.AddPropertyIfAbsent(this.appDomainProperty);
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Environment.CurrentManagedThreadId));
    }
}
