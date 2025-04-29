// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal;

using System;

/// <summary>Represents a locking strategy.</summary>
public interface ILockStrategy : IDisposable
{
    /// <summary>Gets a value indicating whether the thread can read.</summary>
    bool ThreadCanRead { get; }

    /// <summary>Gets a value indicating whether the thread can write.</summary>
    bool ThreadCanWrite { get; }

    /// <summary>Gets a value indicating whether the lock strategy supports concurrent reads.</summary>
    bool SupportsConcurrentReads { get; }

    /// <summary>Gets a read lock.</summary>
    /// <returns><see cref="ISharedCollectionLock"/>.</returns>
    ISharedCollectionLock GetReadLock();

    /// <summary>Gets a read lock with the specified timeout.</summary>
    /// <param name="timeout">The timeout for this lock.</param>
    /// <returns><see cref="ISharedCollectionLock"/>.</returns>
    ISharedCollectionLock GetReadLock(TimeSpan timeout);

    /// <summary>Gets a write lock.</summary>
    /// <returns><see cref="ISharedCollectionLock"/>.</returns>
    ISharedCollectionLock GetWriteLock();

    /// <summary>Gets a write lock with the specified timeout.</summary>
    /// <param name="timeout">The timeout for this lock.</param>
    /// <returns><see cref="ISharedCollectionLock"/>.</returns>
    ISharedCollectionLock GetWriteLock(TimeSpan timeout);
}
