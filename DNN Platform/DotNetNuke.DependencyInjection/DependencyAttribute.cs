// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.DependencyInjection;

using System;

/// <summary>Dependency used for property injection in Action Filters.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DependencyAttribute : Attribute
{
}
