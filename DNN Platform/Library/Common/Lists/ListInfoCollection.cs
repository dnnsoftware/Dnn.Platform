// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Lists;

using System;
using System.Collections;

using DotNetNuke.Instrumentation;

/// <summary>Represents a collection of <see cref="ListInfo"/>.</summary>
[Serializable]
public class ListInfoCollection : CollectionBase
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListInfoCollection));
    private readonly Hashtable mKeyIndexLookup = new Hashtable();

    /// <summary>Gets the children from a parent name.</summary>
    /// <param name="parentName">The name of the parent.</param>
    /// <returns>A <see cref="ListInfo"/>.</returns>
    public ListInfo GetChildren(string parentName)
    {
        return (ListInfo)this.Item(parentName);
    }

    /// <summary>Adds a list to the collection.</summary>
    /// <param name="key">The key of the list.</param>
    /// <param name="value">The value of the object.</param>
    public void Add(string key, object value)
    {
        int index;

        // <tam:note key to be lowercase for appropiated seeking>
        try
        {
            index = this.List.Add(value);
            this.mKeyIndexLookup.Add(key.ToLowerInvariant(), index);
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
        }
    }

    /// <summary>Gets the item at the specified index.</summary>
    /// <param name="index">The index of the item to get.</param>
    /// <returns>The item.</returns>
    public object Item(int index)
    {
        try
        {
            object obj;
            obj = this.List[index];
            return obj;
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return null;
        }
    }

    /// <summary>Gets a list from a spedific key.</summary>
    /// <param name="key">The key to fetch the list.</param>
    /// <returns>A single list.</returns>
    public object Item(string key)
    {
        int index;
        object obj;

        // Do validation first
        try
        {
            if (this.mKeyIndexLookup[key.ToLowerInvariant()] == null)
            {
                return null;
            }
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return null;
        }

        index = Convert.ToInt32(this.mKeyIndexLookup[key.ToLowerInvariant()]);
        obj = this.List[index];
        return obj;
    }

    /// <summary>Gets a single list.</summary>
    /// <param name="key">The key to fetch the list.</param>
    /// <param name="cache">A value indicating whether to cache this list.</param>
    /// <returns>A list object.</returns>
    public object Item(string key, bool cache)
    {
        int index;
        object obj = null;
        bool itemExists = false;

        // Do validation first
        try
        {
            if (this.mKeyIndexLookup[key.ToLowerInvariant()] != null)
            {
                itemExists = true;
            }
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
        }

        // key will be in format Country.US:Region
        if (!itemExists)
        {
            var ctlLists = new ListController();
            string listName = key.Substring(key.IndexOf(":") + 1);
            string parentKey = key.Replace(listName, string.Empty).TrimEnd(':');
            ListInfo listInfo = ctlLists.GetListInfo(listName, parentKey);

            // the collection has been cache, so add this entry list into it if specified
            if (cache)
            {
                this.Add(listInfo.Key, listInfo);
                return listInfo;
            }
        }
        else
        {
            index = Convert.ToInt32(this.mKeyIndexLookup[key.ToLowerInvariant()]);
            obj = this.List[index];
        }

        return obj;
    }

    /// <summary>Gets the child for a parent key.</summary>
    /// <param name="parentKey">The parent key.</param>
    /// <returns>An ArrayList of <see cref="ListInfo"/>.</returns>
    public ArrayList GetChild(string parentKey)
    {
        var childList = new ArrayList();
        foreach (object child in this.List)
        {
            if (((ListInfo)child).Key.IndexOf(parentKey.ToLowerInvariant()) > -1)
            {
                childList.Add(child);
            }
        }

        return childList;
    }

    /// <summary>Clears the collection.</summary>
    internal new void Clear()
    {
        this.mKeyIndexLookup.Clear();
        base.Clear();
    }
}
