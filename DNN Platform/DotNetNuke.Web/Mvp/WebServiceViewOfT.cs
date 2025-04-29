// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvp;

using System;

using DotNetNuke.Internal.SourceGenerators;

using WebFormsMvp;

[DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
public abstract partial class WebServiceViewOfT<TModel> : ModuleViewBase, IView<TModel>
    where TModel : class, new()
{
    private TModel model;

    /// <inheritdoc/>
    public TModel Model
    {
        get
        {
            if (this.model == null)
            {
                throw new InvalidOperationException(
                    "The Model property is currently null, however it should have been automatically initialized by the presenter. This most likely indicates that no presenter was bound to the control. Check your presenter bindings.");
            }

            return this.model;
        }

        set
        {
            this.model = value;
        }
    }
}
