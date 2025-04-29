// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A contract specifying the ability to access an <see cref="IServiceScope"/> instance.</summary>
public interface IScopeAccessor
{
    /// <summary>Gets the scope.</summary>
    /// <returns>The scope, or <see langword="null"/> if there is no scope to get.</returns>
    IServiceScope GetScope();
}
