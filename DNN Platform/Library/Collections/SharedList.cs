// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>An <see cref="IList{T}"/> implementation designed to be shared across threads.</summary>
/// <typeparam name="T">The type of value in the list.</typeparam>
public class SharedList<T> : IList<T>, IDisposable
{
    private readonly List<T> list = new List<T>();
    private ILockStrategy lockStrategy;

    private bool isDisposed;

    /// <summary>Initializes a new instance of the <see cref="SharedList{T}"/> class.</summary>
    public SharedList()
        : this(LockingStrategy.ReaderWriter)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SharedList{T}"/> class.</summary>
    /// <param name="lockStrategy">The locking strategy to use.</param>
    public SharedList(ILockStrategy lockStrategy)
    {
        this.lockStrategy = lockStrategy;
    }

    /// <summary>Initializes a new instance of the <see cref="SharedList{T}"/> class.</summary>
    /// <param name="strategy">The locking strategy to use.</param>
    public SharedList(LockingStrategy strategy)
        : this(LockingStrategyFactory.Create(strategy))
    {
    }

    /// <summary>Finalizes an instance of the <see cref="SharedList{T}"/> class.</summary>
    ~SharedList()
    {
        this.Dispose(false);
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this.list.Count;
        }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
        get
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return ((ICollection<T>)this.list).IsReadOnly;
        }
    }

    /// <summary>Gets the backing list to use.</summary>
    internal IList<T> BackingList
    {
        get
        {
            return this.list;
        }
    }

    /// <inheritdoc/>
    public T this[int index]
    {
        get
        {
            this.EnsureNotDisposed();
            this.EnsureReadAccess();
            return this.list[index];
        }

        set
        {
            this.EnsureNotDisposed();
            this.EnsureWriteAccess();
            this.list[index] = value;
        }
    }

    /// <inheritdoc/>
    public void Add(T item)
    {
        this.EnsureNotDisposed();
        this.EnsureWriteAccess();
        this.list.Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        this.EnsureNotDisposed();
        this.EnsureWriteAccess();
        this.list.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
        this.EnsureNotDisposed();
        this.EnsureReadAccess();
        return this.list.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        this.EnsureNotDisposed();
        this.EnsureReadAccess();
        this.list.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        this.EnsureNotDisposed();
        this.EnsureWriteAccess();
        return this.list.Remove(item);
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        this.EnsureNotDisposed();
        this.EnsureReadAccess();
        return this.list.GetEnumerator();
    }

    /// <inheritdoc/>
    public int IndexOf(T item)
    {
        this.EnsureNotDisposed();
        this.EnsureReadAccess();
        return this.list.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        this.EnsureNotDisposed();
        this.EnsureWriteAccess();
        this.list.Insert(index, item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        this.EnsureNotDisposed();
        this.EnsureWriteAccess();
        this.list.RemoveAt(index);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Ensures the instance is not disposed.</summary>
    public void EnsureNotDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException("SharedList");
        }
    }

    /// <summary>Gets a read lock on the resource.</summary>
    /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
    public ISharedCollectionLock GetReadLock()
    {
        return this.GetReadLock(TimeSpan.FromMilliseconds(-1));
    }

    /// <summary>Gets a read lock on the resource for the specified amount of time.</summary>
    /// <param name="timeOut">The amount of time to lock for.</param>
    /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
    public ISharedCollectionLock GetReadLock(TimeSpan timeOut)
    {
        this.EnsureNotDisposed();
        return this.lockStrategy.GetReadLock(timeOut);
    }

    /// <summary>Gets a read lock on the resource for the specified amount of time.</summary>
    /// <param name="millisecondTimeout">The number of milliseconds to lock for.</param>
    /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
    public ISharedCollectionLock GetReadLock(int millisecondTimeout)
    {
        return this.GetReadLock(TimeSpan.FromMilliseconds(millisecondTimeout));
    }

    /// <summary>Gets a write lock on the resource for the specified amount of time.</summary>
    /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
    public ISharedCollectionLock GetWriteLock()
    {
        return this.GetWriteLock(TimeSpan.FromMilliseconds(-1));
    }

    /// <summary>Gets a write lock on the resource for the specified amount of time.</summary>
    /// <param name="timeOut">The amount of time to lock for.</param>
    /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
    public ISharedCollectionLock GetWriteLock(TimeSpan timeOut)
    {
        this.EnsureNotDisposed();
        return this.lockStrategy.GetWriteLock(timeOut);
    }

    /// <summary>Gets a write lock on the resource for the specified amount of time.</summary>
    /// <param name="millisecondTimeout">The number of milliseconds to lock for.</param>
    /// <returns>An <see cref="ISharedCollectionLock"/> instance.</returns>
    public ISharedCollectionLock GetWriteLock(int millisecondTimeout)
    {
        return this.GetWriteLock(TimeSpan.FromMilliseconds(millisecondTimeout));
    }

    /// <summary>Gets an enumerator to iterate through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator GetEnumerator1()
    {
        return this.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator1();
    }

    /// <summary>Disposes this instance resources.</summary>
    /// <param name="disposing">Indicates if this instance is currently disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                // dispose managed state (managed objects).
            }

            this.lockStrategy.Dispose();
            this.lockStrategy = null;
        }

        this.isDisposed = true;
    }

    private void EnsureReadAccess()
    {
        if (!this.lockStrategy.ThreadCanRead)
        {
            throw new ReadLockRequiredException();
        }
    }

    private void EnsureWriteAccess()
    {
        if (!this.lockStrategy.ThreadCanWrite)
        {
            throw new WriteLockRequiredException();
        }
    }
}
