// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.DependencyInjection;

using System;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A <see cref="Lazy{T}"/> wrapper which allows requesting <see cref="Lazy{T}"/> via dependency injection and delaying instantiation.</summary>
/// <typeparam name="T">The type of service to conditionally instantiate.</typeparam>
public class LazyWrapper<T> : Lazy<T>
{
    /// <summary>Initializes a new instance of the <see cref="LazyWrapper{T}"/> class.</summary>
    /// <param name="serviceProvider">The DI container.</param>
    public LazyWrapper(IServiceProvider serviceProvider)
        : base(serviceProvider.GetService<T>)
    {
    }
}
