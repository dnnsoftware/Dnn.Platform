// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvp
{
    using DotNetNuke.Internal.SourceGenerators;

    using WebFormsMvp;

    /// <summary>Represents a class that is a view for a web service with a strongly typed view model in a Web Forms Model-View-Presenter application.</summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial interface IWebServiceView<TModel> : IWebServiceViewBase, IView<TModel>
        where TModel : class, new()
    {
    }
}
