// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections.Internal;

/// <summary>Represents a locking strategy.</summary>
public enum LockingStrategy
{
    /// <summary>A read/write locking strategy.</summary>
    ReaderWriter = 0,

    /// <summary>An exclusive strategy.</summary>
    Exclusive,
}
