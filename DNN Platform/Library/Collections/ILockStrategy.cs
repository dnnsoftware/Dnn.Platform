// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

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
