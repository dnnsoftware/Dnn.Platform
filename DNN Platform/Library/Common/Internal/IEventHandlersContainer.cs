// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal;

using System;
using System.Collections.Generic;

/// <summary>An interface for containing event handlers.</summary>
/// <typeparam name="T">The type of the event handler container.</typeparam>
internal interface IEventHandlersContainer<T>
{
    /// <summary>Gets the event handlers.</summary>
    IEnumerable<Lazy<T>> EventHandlers { get; }
}
