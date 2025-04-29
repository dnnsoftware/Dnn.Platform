// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists;

using System;
using System.Collections;
using System.ComponentModel;

using DotNetNuke.Instrumentation;
using DotNetNuke.Internal.SourceGenerators;

/// <summary>Represents a collection of list entries.</summary>
[Serializable]
[DnnDeprecated(6, 0, 1, "Replaced by using generic collections of ListEntryInfo objects", RemovalVersion = 10)]
[EditorBrowsable(EditorBrowsableState.Never)]
public partial class ListEntryInfoCollection : CollectionBase
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListEntryInfoCollection));
    private readonly Hashtable keyIndexLookup = new Hashtable();

    /// <summary>Gets the entry at the given index.</summary>
    /// <param name="index">The index of the entry to get.</param>
    /// <returns>A single entry.</returns>
    public ListEntryInfo Item(int index)
    {
        try
        {
            return (ListEntryInfo)this.List[index];
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return null;
        }
    }

    /// <summary>Gets the entry by its key.</summary>
    /// <param name="key">The key of the entry.</param>
    /// <returns>A single list entry.</returns>
    public ListEntryInfo Item(string key)
    {
        int index;

        // <tam:note key to be lowercase for appropiated seeking>
        try
        {
            if (this.keyIndexLookup[key.ToLowerInvariant()] == null)
            {
                return null;
            }
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return null;
        }

        index = Convert.ToInt32(this.keyIndexLookup[key.ToLowerInvariant()]);
        return (ListEntryInfo)this.List[index];
    }

    /// <summary>Gets the children by its parent name.</summary>
    /// <param name="parentName">The name of the parent.</param>
    /// <returns>A single list entry.</returns>
    public ListEntryInfo GetChildren(string parentName)
    {
        return this.Item(parentName);
    }

    /// <summary>Adds an entry to the collection.</summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    public void Add(string key, ListEntryInfo value)
    {
        int index;

        // Do validation first
        try
        {
            index = this.List.Add(value);
            this.keyIndexLookup.Add(key.ToLowerInvariant(), index);
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
        }
    }

    /// <summary>Clears the collection.</summary>
    internal new void Clear()
    {
        this.keyIndexLookup.Clear();
        base.Clear();
    }
}
