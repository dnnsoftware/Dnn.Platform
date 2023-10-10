// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Collections;

using System.Collections.Generic;

/// <inheritdoc cref="IList{T}"/>
public interface IObjectList<T> : IList<T>
{
    /// <summary>
    /// Adds a new instance of <typeparamref name="T"/> to the list.
    /// </summary>
    /// <returns>The new instance.</returns>
    T AddNew();

    /// <summary>
    /// Adds a range of items to the list.
    /// </summary>
    /// <param name="items">The items to add.</param>
    void AddRange(IEnumerable<T> items);
}
