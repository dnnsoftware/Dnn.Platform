// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt;

using DotNetNuke.Abstractions.Users;

/// <summary>Interface implemented by all commands.</summary>
public interface IConsoleCommand
{
    /// <summary>Gets the validation message for the user input. If the user input is invalid, specify what is wrong with it.</summary>
    string ValidationMessage { get; }

    /// <summary>Gets the local resx file to use to retrieve command description, help text and parameter descriptions.</summary>
    string LocalResourceFile { get; }

    /// <summary>Gets the help text for the command. This is used when retrieving help for the command.</summary>
    string ResultHtml { get; }

    /// <summary>
    /// Initializes the command when invoked by the client. Note that you can opt to override this, but you should
    /// call <c>base.Initialize()</c> to ensure all base values are loaded.
    /// </summary>
    /// <param name="args">Raw argument list passed by the client.</param>
    /// <param name="portalSettings">PortalSettings for the portal we're operating under or if PortalId is specified, that portal.</param>
    /// <param name="userInfo">Current user.</param>
    /// <param name="activeTabId">Current page/tab.</param>
    void Initialize(string[] args, Portals.IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId);

    /// <summary>The main method of the command which executes it.</summary>
    /// <returns>A class used by the client to display results.</returns>
    IConsoleResultModel Run();

    /// <summary>Specify whether the user input for the command is valid.</summary>
    /// <returns>True is the command can execute.</returns>
    bool IsValid();
}
