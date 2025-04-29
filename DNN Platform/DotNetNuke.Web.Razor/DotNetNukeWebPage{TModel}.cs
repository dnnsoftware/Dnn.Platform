// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Razor;

using System;

using DotNetNuke.Common;
using DotNetNuke.Internal.SourceGenerators;

using Microsoft.Extensions.DependencyInjection;

[DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
public abstract partial class DotNetNukeWebPage<TModel> : DotNetNukeWebPage
    where TModel : class
{
    private TModel model;

    public DotNetNukeWebPage()
    {
        var model = Globals.DependencyProvider.GetService<TModel>();
        this.Model = model ?? Activator.CreateInstance<TModel>();
    }

    public new TModel Model
    {
        get { return this.PageContext?.Model as TModel ?? this.model; }
        set { this.model = value; }
    }
}
