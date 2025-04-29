// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Prompt;

using System;
using System.Linq;

using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Prompt;
using DotNetNuke.Abstractions.Users;
using DotNetNuke.Prompt;

using Microsoft.Extensions.DependencyInjection;

/// <summary>This is a (Prompt) Console Command. You should not reference this class directly. It is to be used solely through Prompt.</summary>
[ConsoleCommand("list-services", "Prompt_DebugCategory", "Prompt_ListServices_Description")]
public class ListServices : ConsoleCommand
{
    /// <inheritdoc/>
    public override string LocalResourceFile => Constants.DefaultPromptResourceFile;

    /// <inheritdoc/>
    public override void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
    {
        base.Initialize(args, portalSettings, userInfo, activeTabId);
        if (!userInfo.IsSuperUser)
        {
            this.AddMessage(this.LocalizeString("Prompt_ListServices_Unauthorized"));
        }

        this.ParseParameters(this);
    }

    /// <inheritdoc/>
    public override IConsoleResultModel Run()
    {
        if (!this.User.IsSuperUser)
        {
            return new ConsoleErrorResultModel(this.LocalizeString("Prompt_ListServices_Unauthorized"));
        }

        var services = DependencyInjectionInitialize.ServiceCollection.Select(
                descriptor => new
                {
                    LifeTime = descriptor.Lifetime.ToString("G"),
                    Service = this.GetTypeName(descriptor.ServiceType),
                    Implementation = this.GetImplementationText(descriptor),
                })
            .OrderBy(desc => desc.Service.Output)
            .ThenBy(desc => desc.Implementation.Output)
            .ToList();
        return new ConsoleResultModel
        {
            Data = services,
            Records = services.Count,
        };
    }

    private IConsoleOutput GetImplementationText(ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null)
        {
            return new HtmlOutput(this.LocalizeString("Prompt_ListServices_ImplementationInstance"));
        }

        if (descriptor.ImplementationFactory != null)
        {
            return new HtmlOutput(this.LocalizeString("Prompt_ListServices_ImplementationFactory"));
        }

        return this.GetTypeName(descriptor.ImplementationType);
    }

    private IConsoleOutput GetTypeName(Type type)
    {
        if (type == null)
        {
            return new HtmlOutput(this.LocalizeString("Prompt_ListServices_None"));
        }

        return new TextOutput(type.FullName ?? type.Name);
    }
}
