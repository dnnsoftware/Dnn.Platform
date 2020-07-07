// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchItemInfoCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Represents a collection of <see cref="SearchItemInfo">SearchItemInfo</see> objects.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    [Serializable]
    public class SearchItemInfoCollection : CollectionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchItemInfoCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> class.
        /// </summary>
        public SearchItemInfoCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchItemInfoCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> class containing the elements of the specified source collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> with which to initialize the collection.</param>
        public SearchItemInfoCollection(SearchItemInfoCollection value)
        {
            this.AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchItemInfoCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> class containing the specified array of <see cref="SearchItemInfo">SearchItemInfo</see> objects.
        /// </summary>
        /// <param name="value">An array of <see cref="SearchItemInfo">SearchItemInfo</see> objects with which to initialize the collection. </param>
        public SearchItemInfoCollection(SearchItemInfo[] value)
        {
            this.AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchItemInfoCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchItemInfoCollection">SearchItemInfoCollectionSearchItemInfoCollection</see> class containing the specified array of <see cref="SearchItemInfo">SearchItemInfo</see> objects.
        /// </summary>
        /// <param name="value">An arraylist of <see cref="SearchItemInfo">SearchItemInfo</see> objects with which to initialize the collection. </param>
        public SearchItemInfoCollection(ArrayList value)
        {
            this.AddRange(value);
        }

        /// <summary>
        /// Gets the <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> at the specified index in the collection.
        /// <para>
        /// In VB.Net, this property is the indexer for the <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> class.
        /// </para>
        /// </summary>
        public SearchItemInfo this[int index]
        {
            get
            {
                return (SearchItemInfo)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Add an element of the specified <see cref="SearchItemInfo">SearchItemInfo</see> to the end of the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchItemInfo">SearchItemInfo</see> to add to the collection.</param>
        /// <returns></returns>
        public int Add(SearchItemInfo value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Gets the index in the collection of the specified <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see>, if it exists in the collection.
        /// </summary>
        /// <param name="value">The <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> to locate in the collection.</param>
        /// <returns>The index in the collection of the specified object, if found; otherwise, -1.</returns>
        public int IndexOf(SearchItemInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Add an element of the specified <see cref="SearchItemInfo">SearchItemInfo</see> to the collection at the designated index.
        /// </summary>
        /// <param name="index">An <see cref="int">Integer</see> to indicate the location to add the object to the collection.</param>
        /// <param name="value">An object of type <see cref="SearchItemInfo">SearchItemInfo</see> to add to the collection.</param>
        public void Insert(int index, SearchItemInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Remove the specified object of type <see cref="SearchItemInfo">SearchItemInfo</see> from the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchItemInfo">SearchItemInfo</see> to remove to the collection.</param>
        public void Remove(SearchItemInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the specified <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see>.
        /// </summary>
        /// <param name="value">The <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> to search for in the collection.</param>
        /// <returns><b>true</b> if the collection contains the specified object; otherwise, <b>false</b>.</returns>
        public bool Contains(SearchItemInfo value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Copies the elements of the specified <see cref="SearchItemInfo">SearchItemInfo</see> array to the end of the collection.
        /// </summary>
        /// <param name="value">An array of type <see cref="SearchItemInfo">SearchItemInfo</see> containing the objects to add to the collection.</param>
        public void AddRange(SearchItemInfo[] value)
        {
            for (int i = 0; i <= value.Length - 1; i++)
            {
                this.Add(value[i]);
            }
        }

        /// <summary>
        /// Copies the elements of the specified arraylist to the end of the collection.
        /// </summary>
        /// <param name="value">An arraylist of type <see cref="SearchItemInfo">SearchItemInfo</see> containing the objects to add to the collection.</param>
        public void AddRange(ArrayList value)
        {
            foreach (object obj in value)
            {
                if (obj is SearchItemInfo)
                {
                    this.Add((SearchItemInfo)obj);
                }
            }
        }

        /// <summary>
        /// Adds the contents of another <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> to the end of the collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchItemInfoCollection">SearchItemInfoCollection</see> containing the objects to add to the collection. </param>
        public void AddRange(SearchItemInfoCollection value)
        {
            for (int i = 0; i <= value.Count - 1; i++)
            {
                this.Add((SearchItemInfo)value.List[i]);
            }
        }

        /// <summary>
        /// Copies the collection objects to a one-dimensional <see cref="T:System.Array">Array</see> instance beginning at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the values copied from the collection.</param>
        /// <param name="index">The index of the array at which to begin inserting.</param>
        public void CopyTo(SearchItemInfo[] array, int index)
        {
            this.List.CopyTo(array, index);
        }

        /// <summary>
        /// Creates a one-dimensional <see cref="T:System.Array">Array</see> instance containing the collection items.
        /// </summary>
        /// <returns>Array of type SearchItemInfo.</returns>
        public SearchItemInfo[] ToArray()
        {
            var arr = new SearchItemInfo[this.Count];
            this.CopyTo(arr, 0);

            return arr;
        }

        public SearchItemInfoCollection ModuleItems(int ModuleId)
        {
            var retValue = new SearchItemInfoCollection();
            foreach (SearchItemInfo info in this)
            {
                if (info.ModuleId == ModuleId)
                {
                    retValue.Add(info);
                }
            }

            return retValue;
        }
    }
}
