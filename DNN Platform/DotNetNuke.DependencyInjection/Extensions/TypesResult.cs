// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.DependencyInjection.Extensions;

using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>The result of a request to get types from assemblies.</summary>
public class TypesResult
{
    /// <summary>Initializes a new instance of the <see cref="TypesResult"/> class.</summary>
    /// <param name="types">The types.</param>
    /// <param name="loadExceptions">The <see cref="ReflectionTypeLoadException"/> instances that were occurred while loading the assemblies.</param>
    /// <param name="otherExceptions">The other exceptions that occurred while loading the assemblies.</param>
    public TypesResult(IReadOnlyCollection<Type> types, IReadOnlyDictionary<Assembly, ReflectionTypeLoadException> loadExceptions, IReadOnlyDictionary<Assembly, Exception> otherExceptions)
    {
        this.Types = types;
        this.LoadExceptions = loadExceptions;
        this.OtherExceptions = otherExceptions;
    }

    /// <summary>Gets the types.</summary>
    public IReadOnlyCollection<Type> Types { get; }

    /// <summary>Gets the <see cref="ReflectionTypeLoadException"/> instances that were occurred while loading the assemblies.</summary>
    public IReadOnlyDictionary<Assembly, ReflectionTypeLoadException> LoadExceptions { get; }

    /// <summary>Gets any other exceptions that occurred while loading the assemblies.</summary>
    public IReadOnlyDictionary<Assembly, Exception> OtherExceptions { get; }
}
