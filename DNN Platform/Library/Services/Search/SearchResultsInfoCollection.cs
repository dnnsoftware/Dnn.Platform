// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using System;
using System.Collections;

using DotNetNuke.Internal.SourceGenerators;

/// <summary>Represents a collection of <see cref="SearchResultsInfo"/> objects.</summary>
[DnnDeprecated(7, 1, 0, "No longer used in the Search infrastructure", RemovalVersion = 10)]
[Serializable]
public partial class SearchResultsInfoCollection : CollectionBase
{
    /// <summary>Initializes a new instance of the <see cref="SearchResultsInfoCollection"/> class.</summary>
    public SearchResultsInfoCollection()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SearchResultsInfoCollection"/> class containing the elements of the specified source collection.</summary>
    /// <param name="value">A <see cref="SearchResultsInfoCollection"/> with which to initialize the collection.</param>
    public SearchResultsInfoCollection(SearchResultsInfoCollection value)
    {
        this.AddRange(value);
    }

    /// <summary>Initializes a new instance of the <see cref="SearchResultsInfoCollection"/> class containing the specified array of <see cref="SearchResultsInfo"/> objects.</summary>
    /// <param name="value">An array of <see cref="SearchResultsInfo"/> objects with which to initialize the collection.</param>
    public SearchResultsInfoCollection(SearchResultsInfo[] value)
    {
        this.AddRange(value);
    }

    /// <summary>Initializes a new instance of the <see cref="SearchResultsInfoCollection"/> class containing the specified array of <see cref="SearchResultsInfo"/> objects.</summary>
    /// <param name="value">An array of <see cref="SearchResultsInfo"/> objects with which to initialize the collection.</param>
    public SearchResultsInfoCollection(ArrayList value)
    {
        this.AddRange(value);
    }

    /// <summary>
    /// Gets the <see cref="SearchResultsInfoCollection"/> at the specified index in the collection.
    /// <para>In VB.Net, this property is the indexer for the <see cref="SearchResultsInfoCollection"/> class.</para>
    /// </summary>
    public SearchResultsInfo this[int index]
    {
        get
        {
            return (SearchResultsInfo)this.List[index];
        }

        set
        {
            this.List[index] = value;
        }
    }

    /// <summary>Add an element of the specified <see cref="SearchResultsInfo"/> to the end of the collection.</summary>
    /// <param name="value">An object of type <see cref="SearchResultsInfo"/> to add to the collection.</param>
    /// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
    public int Add(SearchResultsInfo value)
    {
        return this.List.Add(value);
    }

    /// <summary>Gets the index in the collection of the specified <see cref="SearchResultsInfoCollection"/>, if it exists in the collection.</summary>
    /// <param name="value">The <see cref="SearchResultsInfoCollection"/> to locate in the collection.</param>
    /// <returns>The index in the collection of the specified object, if found; otherwise, -1.</returns>
    public int IndexOf(SearchResultsInfo value)
    {
        return this.List.IndexOf(value);
    }

    /// <summary>Add an element of the specified <see cref="SearchResultsInfo"/> to the collection at the designated index.</summary>
    /// <param name="index">An <see cref="int">Integer</see> to indicate the location to add the object to the collection.</param>
    /// <param name="value">An object of type <see cref="SearchResultsInfo"/> to add to the collection.</param>
    public void Insert(int index, SearchResultsInfo value)
    {
        this.List.Insert(index, value);
    }

    /// <summary>Remove the specified object of type <see cref="SearchResultsInfo"/> from the collection.</summary>
    /// <param name="value">An object of type <see cref="SearchResultsInfo"/> to remove to the collection.</param>
    public void Remove(SearchResultsInfo value)
    {
        this.List.Remove(value);
    }

    /// <summary>Gets a value indicating whether the collection contains the specified <see cref="SearchResultsInfoCollection"/>.</summary>
    /// <param name="value">The <see cref="SearchResultsInfoCollection"/> to search for in the collection.</param>
    /// <returns><b>true</b> if the collection contains the specified object; otherwise, <b>false</b>.</returns>
    public bool Contains(SearchResultsInfo value)
    {
        return this.List.Contains(value);
    }

    /// <summary>Copies the elements of the specified <see cref="SearchResultsInfo" /> array to the end of the collection.</summary>
    /// <param name="value">An array of type <see cref="SearchResultsInfo" /> containing the objects to add to the collection.</param>
    public void AddRange(SearchResultsInfo[] value)
    {
        for (int i = 0; i <= value.Length - 1; i++)
        {
            this.Add(value[i]);
        }
    }

    /// <summary>Copies the elements of the specified <see cref="ArrayList"/> to the end of the collection.</summary>
    /// <param name="value">An <see cref="ArrayList"/> of type <see cref="SearchResultsInfo" /> containing the objects to add to the collection.</param>
    public void AddRange(ArrayList value)
    {
        foreach (object obj in value)
        {
            if (obj is SearchResultsInfo)
            {
                this.Add((SearchResultsInfo)obj);
            }
        }
    }

    /// <summary>Adds the contents of another <see cref="SearchResultsInfoCollection"/> to the end of the collection.</summary>
    /// <param name="value">A <see cref="SearchResultsInfoCollection"/> containing the objects to add to the collection. </param>
    public void AddRange(SearchResultsInfoCollection value)
    {
        for (int i = 0; i <= value.Count - 1; i++)
        {
            this.Add((SearchResultsInfo)value.List[i]);
        }
    }

    /// <summary>Copies the collection objects to a one-dimensional <see cref="Array" /> instance beginning at the specified index.</summary>
    /// <param name="array">The one-dimensional <see cref="Array" /> that is the destination of the values copied from the collection.</param>
    /// <param name="index">The index of the array at which to begin inserting.</param>
    public void CopyTo(SearchResultsInfo[] array, int index)
    {
        this.List.CopyTo(array, index);
    }

    /// <summary>Creates a one-dimensional <see cref="Array" /> instance containing the collection items.</summary>
    /// <returns>Array of type SearchResultsInfo.</returns>
    public SearchResultsInfo[] ToArray()
    {
        var arr = new SearchResultsInfo[this.Count];
        this.CopyTo(arr, 0);
        return arr;
    }
}
