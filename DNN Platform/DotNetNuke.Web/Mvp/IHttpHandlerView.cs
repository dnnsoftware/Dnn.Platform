// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvp;

using DotNetNuke.Internal.SourceGenerators;

using WebFormsMvp;

/// <summary>Http handler of an WebForms MVP View.</summary>
[DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
public partial interface IHttpHandlerView : IView
{
}
