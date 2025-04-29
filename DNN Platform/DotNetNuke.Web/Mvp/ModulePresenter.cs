// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvp;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
public abstract partial class ModulePresenter<TView> : ModulePresenterBase<TView>
    where TView : class, IModuleView
{
    /// <summary>Initializes a new instance of the <see cref="ModulePresenter{TView}"/> class.</summary>
    /// <param name="view">The view.</param>
    protected ModulePresenter(TView view)
        : base(view)
    {
    }
}
