// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging;

/// <summary>The exception info used for logging.</summary>
public interface IExceptionInfo
{
    /// <summary>Gets or sets the assembly version.</summary>
    string AssemblyVersion { get; set; }

    /// <summary>Gets or sets the portal id.</summary>
    int PortalId { get; set; }

    /// <summary>Gets or sets the user id.</summary>
    int UserId { get; set; }

    /// <summary>Gets or sets the tab id.</summary>
    int TabId { get; set; }

    /// <summary>Gets or sets the raw URL.</summary>
    string RawUrl { get; set; }

    /// <summary>Gets or sets the referrer.</summary>
    string Referrer { get; set; }

    /// <summary>Gets or sets the User Agent.</summary>
    string UserAgent { get; set; }

    /// <summary>Gets or sets the Exception Has.</summary>
    string ExceptionHash { get; set; }

    /// <summary>Gets or sets the message.</summary>
    string Message { get; set; }

    /// <summary>Gets or sets the stack trace.</summary>
    string StackTrace { get; set; }

    /// <summary>Gets or sets inner message.</summary>
    string InnerMessage { get; set; }

    /// <summary>Gets or sets the inner stack trace.</summary>
    string InnerStackTrace { get; set; }

    /// <summary>Gets or sets the source.</summary>
    string Source { get; set; }

    /// <summary>Gets or sets the file name.</summary>
    string FileName { get; set; }

    /// <summary>gets or sets the file line number.</summary>
    int FileLineNumber { get; set; }

    /// <summary>gets or sets the file column number.</summary>
    int FileColumnNumber { get; set; }

    /// <summary>Gets or sets the method.</summary>
    string Method { get; set; }
}
