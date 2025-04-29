// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints.Filters;

public class FilterByUnauthenticated : IExtensionPointFilter
{
    private readonly bool isAuthenticated;

    /// <summary>Initializes a new instance of the <see cref="FilterByUnauthenticated"/> class.</summary>
    /// <param name="isAuthenticated"></param>
    public FilterByUnauthenticated(bool isAuthenticated)
    {
        this.isAuthenticated = isAuthenticated;
    }

    /// <inheritdoc/>
    public bool Condition(IExtensionPointData m)
    {
        return this.isAuthenticated || !m.DisableUnauthenticated;
    }
}
