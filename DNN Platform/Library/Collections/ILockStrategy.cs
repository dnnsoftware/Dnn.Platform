

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;

namespace DotNetNuke.Collections.Internal
{
    public interface ILockStrategy : IDisposable
    {
        bool ThreadCanRead { get; }

        bool ThreadCanWrite { get; }

        bool SupportsConcurrentReads { get; }

        ISharedCollectionLock GetReadLock();

        ISharedCollectionLock GetReadLock(TimeSpan timeout);

        ISharedCollectionLock GetWriteLock();

        ISharedCollectionLock GetWriteLock(TimeSpan timeout);
    }
}
