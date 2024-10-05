// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Abstractions.Collections;

/// <inheritdoc cref="IObjectList{TInterface}"/>
/// <remarks>
/// This class is used to abstract a list of objects of type <typeparamref name="TInterface"/> to a list of objects of
/// type <typeparamref name="TImplementation"/>.
/// </remarks>
public class AbstractionList<TInterface, TImplementation> : IObjectList<TInterface>
    where TImplementation : TInterface, new()
{
    private readonly IList list;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractionList{TInterface, TImplementation}"/> class.
    /// </summary>
    /// <param name="list">The list.</param>
    public AbstractionList(IList list)
    {
        this.list = list;
    }

    /// <inheritdoc />
    public int Count => this.list.Count;

    /// <inheritdoc />
    public bool IsReadOnly => this.list.IsReadOnly;

    /// <inheritdoc />
    public TInterface this[int index]
    {
        get => (TInterface)this.list[index];
        set => this.list[index] = value;
    }

    /// <inheritdoc />
    public TInterface AddNew()
    {
        var item = new TImplementation();
        this.list.Add(item);
        return item;
    }

    /// <inheritdoc cref="IObjectList{T}.AddRange"/>
    public void AddRange(IEnumerable<TInterface> items)
    {
        foreach (var item in items)
        {
            this.Add(item);
        }
    }

    /// <inheritdoc />
    public IEnumerator<TInterface> GetEnumerator()
    {
        return this.list.Cast<TInterface>().GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <inheritdoc />
    public void Add(TInterface item)
    {
        if (item is not TImplementation implementation)
        {
            throw new System.InvalidCastException();
        }

        this.list.Add(implementation);
    }

    /// <inheritdoc />
    public void Clear()
    {
        this.list.Clear();
    }

    /// <inheritdoc />
    public bool Contains(TInterface item)
    {
        return this.list.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(TInterface[] array, int arrayIndex)
    {
        this.list.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public bool Remove(TInterface item)
    {
        var count = this.list.Count;
        this.list.Remove(item);
        return count != this.list.Count;
    }

    /// <inheritdoc />
    public int IndexOf(TInterface item)
    {
        return this.list.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, TInterface item)
    {
        this.list.Insert(index, item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        this.list.RemoveAt(index);
    }
}
