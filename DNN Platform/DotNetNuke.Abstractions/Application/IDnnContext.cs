// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Application;

/// <summary>Defines the context for the environment of the DotNetNuke application.</summary>
public interface IDnnContext
{
    /// <summary>Gets get the application.</summary>
    IApplicationInfo Application { get; }
}
