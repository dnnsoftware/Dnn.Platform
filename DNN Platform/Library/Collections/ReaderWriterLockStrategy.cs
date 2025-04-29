// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal;

using System;
using System.Runtime.Serialization;
using System.Threading;

/// <summary>Provides read/write locking.</summary>
[Serializable]
public class ReaderWriterLockStrategy : IDisposable, ILockStrategy
{
    [NonSerialized]
    private ReaderWriterLockSlim @lock;

    private LockRecursionPolicy lockRecursionPolicy;

    private bool isDisposed;

    /// <summary>Initializes a new instance of the <see cref="ReaderWriterLockStrategy"/> class.</summary>
    public ReaderWriterLockStrategy()
        : this(LockRecursionPolicy.NoRecursion)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ReaderWriterLockStrategy"/> class.</summary>
    /// <param name="recursionPolicy">An instance of a <see cref="LockRecursionPolicy"/> to use.</param>
    public ReaderWriterLockStrategy(LockRecursionPolicy recursionPolicy)
    {
        this.lockRecursionPolicy = recursionPolicy;
        this.@lock = new ReaderWriterLockSlim(recursionPolicy);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReaderWriterLockStrategy"/> class.
    /// The special constructor is used to deserialize values.
    /// </summary>
    /// <param name="info">A <see cref="SerializationInfo"/> to use.</param>
    /// <param name="context">The streaming context to use.</param>
    public ReaderWriterLockStrategy(SerializationInfo info, StreamingContext context)
    {
        this.lockRecursionPolicy = (LockRecursionPolicy)info.GetValue("_lockRecursionPolicy", typeof(LockRecursionPolicy));
        this.@lock = new ReaderWriterLockSlim(this.lockRecursionPolicy);
    }

    /// <summary>Finalizes an instance of the <see cref="ReaderWriterLockStrategy"/> class.</summary>
    ~ReaderWriterLockStrategy()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        this.Dispose(false);
    }

    /// <inheritdoc/>
    public bool ThreadCanRead
    {
        get
        {
            this.EnsureNotDisposed();
            return this.Lock.IsReadLockHeld || this.Lock.IsWriteLockHeld;
        }
    }

    /// <inheritdoc/>
    public bool ThreadCanWrite
    {
        get
        {
            this.EnsureNotDisposed();
            return this.Lock.IsWriteLockHeld;
        }
    }

    /// <inheritdoc/>
    public bool SupportsConcurrentReads
    {
        get
        {
            return true;
        }
    }

    private ReaderWriterLockSlim Lock
    {
        get
        {
            return this.@lock ?? (this.@lock = new ReaderWriterLockSlim(this.lockRecursionPolicy));
        }
    }

    /// <summary>
    /// Implement this method to serialize data. The method is called
    /// on serialization.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to use.</param>
    /// <param name="context">The <see cref="StreamingContext"/> to use.</param>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        // Use the AddValue method to specify serialized values.
        info.AddValue("_lockRecursionPolicy", this.lockRecursionPolicy, typeof(LockRecursionPolicy));
    }

    /// <inheritdoc/>
    public ISharedCollectionLock GetReadLock()
    {
        return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
    }

    /// <inheritdoc/>
    public ISharedCollectionLock GetReadLock(TimeSpan timeout)
    {
        this.EnsureNotDisposed();
        if ((this.Lock.RecursionPolicy == LockRecursionPolicy.NoRecursion && this.Lock.IsReadLockHeld) ||
            this.Lock.TryEnterReadLock(timeout))
        {
            return new ReaderWriterSlimLock(this.Lock);
        }
        else
        {
            throw new ApplicationException("ReaderWriterLockStrategy.GetReadLock timed out");
        }
    }

    /// <inheritdoc/>
    public ISharedCollectionLock GetWriteLock()
    {
        return this.GetWriteLock(TimeSpan.FromMilliseconds(-1));
    }

    /// <inheritdoc/>
    public ISharedCollectionLock GetWriteLock(TimeSpan timeout)
    {
        this.EnsureNotDisposed();
        if ((this.Lock.RecursionPolicy == LockRecursionPolicy.NoRecursion && this.Lock.IsWriteLockHeld) ||
            this.Lock.TryEnterWriteLock(timeout))
        {
            return new ReaderWriterSlimLock(this.Lock);
        }
        else
        {
            throw new ApplicationException("ReaderWriterLockStrategy.GetWriteLock timed out");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes this instance resources.</summary>
    /// <param name="disposing">A value indicating whether this instance is currently disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                // dispose managed state (managed objects).
            }

            if (this.@lock != null)
            {
                this.@lock.Dispose();
                this.@lock = null;
            }
        }

        this.isDisposed = true;
    }

    private void EnsureNotDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException("ReaderWriterLockStrategy");
        }
    }
}
