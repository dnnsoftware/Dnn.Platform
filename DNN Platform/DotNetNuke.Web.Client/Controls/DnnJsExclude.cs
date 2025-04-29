// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement;

using DotNetNuke.Web.Client.Controls;

public class DnnJsExclude : ClientResourceExclude
{
    /// <summary>Initializes a new instance of the <see cref="DnnJsExclude"/> class.</summary>
    public DnnJsExclude()
    {
        this.DependencyType = ClientDependency.Core.ClientDependencyType.Javascript;
    }
}
