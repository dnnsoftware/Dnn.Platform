// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>An implementation of <see cref="CollectionBase"/> which also implements <see cref="IList{T}"/>.</summary>
/// <remarks>
/// This is primarily for "upgrading" existing strongly-typed collections to implement the generic <see cref="IList{T}"/> interface,
/// using generic collections directly is a better choice to new APIs.
/// </remarks>
/// <typeparam name="TElement">The type of element the collection holds.</typeparam>
[Serializable]
public abstract class GenericCollectionBase<TElement> : CollectionBase, IList<TElement>
{
    /// <inheritdoc />
    public bool IsReadOnly => this.InnerList.IsReadOnly;

    /// <summary>Gets or sets the element at the specified index.</summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. -or- <paramref name="index"/> is equal to or greater than <see cref="CollectionBase.Count"/>.</exception>
    public virtual TElement this[int index]
    {
        get => (TElement)this.List[index];
        set => this.List[index] = value;
    }

    /// <inheritdoc cref="ArrayList.Add"/>
    public int Add(TElement a) => this.InnerList.Add(a);

    /// <inheritdoc cref="ArrayList.Add"/>
    void ICollection<TElement>.Add(TElement a) => this.InnerList.Add(a);

    /// <inheritdoc />
    public bool Remove(TElement item)
    {
        if (!this.Contains(item))
        {
            return false;
        }

        this.InnerList.Remove(item);
        return true;
    }

    /// <inheritdoc />
    public bool Contains(TElement item) => this.InnerList.Contains(item);

    /// <inheritdoc />
    public void CopyTo(TElement[] array, int arrayIndex) => this.InnerList.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public int IndexOf(TElement item) => this.InnerList.IndexOf(item);

    /// <inheritdoc />
    public void Insert(int index, TElement item) => this.InnerList.Insert(index, item);

    /// <inheritdoc />
    IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
    {
        return new GenericEnumerator(this.GetEnumerator());
    }

    private class GenericEnumerator(IEnumerator enumerator) : IEnumerator<TElement>
    {
        /// <inheritdoc />
        public TElement Current => (TElement)enumerator.Current;

        /// <inheritdoc />
        object IEnumerator.Current => this.Current;

        /// <inheritdoc />
        public void Dispose() => (enumerator as IDisposable)?.Dispose();

        /// <inheritdoc />
        public bool MoveNext() => enumerator.MoveNext();

        /// <inheritdoc />
        public void Reset() => enumerator.Reset();
    }
}
