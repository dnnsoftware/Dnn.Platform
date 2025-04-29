// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Application;

using System;
using System.Collections.Generic;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common;
using DotNetNuke.UI.Containers.EventListeners;
using DotNetNuke.UI.Skins.EventListeners;

using Microsoft.Extensions.DependencyInjection;

/// <summary>Defines the context for the environment of the DotNetNuke application.</summary>
public class DotNetNukeContext : IDnnContext
{
    private static IDnnContext current;
    private readonly IApplicationInfo applicationInfo;

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeContext" /> class using the provided application as base.</summary>
    /// <param name="applicationInfo">The application.</param>
    /// <remarks>
    /// This constructor is designed to be used with Dependency Injection.
    /// </remarks>
    public DotNetNukeContext(IApplicationInfo applicationInfo)
    {
        this.applicationInfo = applicationInfo;
        this.ContainerEventListeners = new NaiveLockingList<ContainerEventListener>();
        this.SkinEventListeners = new NaiveLockingList<SkinEventListener>();
    }

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeContext"/> class.</summary>
    /// <remarks>
    /// Initialize using the public constructor for Dependency Injection, this method will be removed.
    /// </remarks>
    [Obsolete("Deprecated in DotNetNuke 9.7.1. This constructor has been replaced by parameterized public constructor which is designed to be used with Dependency Injection. Resolve the new interface 'DotNetNuke.Abstractions.IDnnContext' instead. Scheduled removal in v11.0.0.")]
    protected DotNetNukeContext()
        : this(Globals.DependencyProvider.GetRequiredService<IApplicationInfo>())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeContext"/> class.</summary>
    /// <param name="application">A reference to the .Net application.</param>
    /// <remarks>
    /// Initialize using the public constructor for Dependency Injection, this method will be removed.
    /// </remarks>
    [Obsolete("Deprecated in DotNetNuke 9.7.1. This constructor has been replaced by the overload taking an IApplicationInfo, which should be resolved via Dependency Injection. Scheduled removal in v11.0.0.")]
    protected DotNetNukeContext(Application application)
        : this((IApplicationInfo)application)
    {
    }

    /// <summary>Gets or sets the current app context.</summary>
    public static DotNetNukeContext Current
    {
        get
        {
            if (current == null)
            {
                current = Globals.DependencyProvider.GetRequiredService<IDnnContext>();
            }

            return current is DotNetNukeContext context ? context : default(DotNetNukeContext);
        }

        set
        {
            current = value;
        }
    }

    /// <summary>Gets get the application.</summary>
    public Application Application { get => this.applicationInfo is Application app ? app : new Application(); }

    /// <inheritdoc />
    IApplicationInfo IDnnContext.Application { get => this.applicationInfo; }

    /// <summary>Gets the container event listeners. The listeners will be called in each life cycle of load container.</summary>
    /// <see cref="ContainerEventListener"/>
    /// <seealso cref="DotNetNuke.UI.Containers.Container.OnInit"/>
    /// <seealso cref="DotNetNuke.UI.Containers.Container.OnLoad"/>
    /// <seealso cref="DotNetNuke.UI.Containers.Container.OnPreRender"/>
    /// <seealso cref="DotNetNuke.UI.Containers.Container.OnUnload"/>
    public IList<ContainerEventListener> ContainerEventListeners { get; }

    /// <summary>Gets the skin event listeners. The listeners will be called in each life cycle of load skin.</summary>
    /// <see cref="SkinEventListener"/>
    /// <seealso cref="DotNetNuke.UI.Skins.Skin.OnInit"/>
    /// <seealso cref="DotNetNuke.UI.Skins.Skin.OnLoad"/>
    /// <seealso cref="DotNetNuke.UI.Skins.Skin.OnPreRender"/>
    /// <seealso cref="DotNetNuke.UI.Skins.Skin.OnUnload"/>
    public IList<SkinEventListener> SkinEventListeners { get; }
}
