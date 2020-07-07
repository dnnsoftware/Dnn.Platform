// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System.Collections;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      SearchContentModuleInfoCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Represents a collection of <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> objects.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
#pragma warning disable 0618
    public class SearchContentModuleInfoCollection : CollectionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> class.
        /// </summary>
        public SearchContentModuleInfoCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> class containing the elements of the specified source collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> with which to initialize the collection.</param>
        public SearchContentModuleInfoCollection(SearchContentModuleInfoCollection value)
        {
            this.AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> class containing the specified array of <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> objects.
        /// </summary>
        /// <param name="value">An array of <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> objects with which to initialize the collection. </param>
        public SearchContentModuleInfoCollection(SearchContentModuleInfo[] value)
        {
            this.AddRange(value);
        }

        /// <summary>
        /// Gets the <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> at the specified index in the collection.
        /// <para>
        /// In VB.Net, this property is the indexer for the <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> class.
        /// </para>
        /// </summary>
        public SearchContentModuleInfo this[int index]
        {
            get
            {
                return (SearchContentModuleInfo)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Add an element of the specified <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to the end of the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to add to the collection.</param>
        /// <returns></returns>
        public int Add(SearchContentModuleInfo value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Gets the index in the collection of the specified <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see>, if it exists in the collection.
        /// </summary>
        /// <param name="value">The <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> to locate in the collection.</param>
        /// <returns>The index in the collection of the specified object, if found; otherwise, -1.</returns>
        public int IndexOf(SearchContentModuleInfo value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Add an element of the specified <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to the collection at the designated index.
        /// </summary>
        /// <param name="index">An <see cref="int">Integer</see> to indicate the location to add the object to the collection.</param>
        /// <param name="value">An object of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to add to the collection.</param>
        public void Insert(int index, SearchContentModuleInfo value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Remove the specified object of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> from the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to remove to the collection.</param>
        public void Remove(SearchContentModuleInfo value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the specified <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see>.
        /// </summary>
        /// <param name="value">The <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> to search for in the collection.</param>
        /// <returns><b>true</b> if the collection contains the specified object; otherwise, <b>false</b>.</returns>
        public bool Contains(SearchContentModuleInfo value)
        {
            // If value is not of type SearchContentModuleInfo, this will return false.
            return this.List.Contains(value);
        }

        /// <summary>
        /// Copies the elements of the specified <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> array to the end of the collection.
        /// </summary>
        /// <param name="value">An array of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> containing the objects to add to the collection.</param>
        public void AddRange(SearchContentModuleInfo[] value)
        {
            for (int i = 0; i <= value.Length - 1; i++)
            {
                this.Add(value[i]);
            }
        }

        /// <summary>
        /// Adds the contents of another <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> to the end of the collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> containing the objects to add to the collection. </param>
        public void AddRange(SearchContentModuleInfoCollection value)
        {
            for (int i = 0; i <= value.Count - 1; i++)
            {
                this.Add((SearchContentModuleInfo)value.List[i]);
            }
        }

        /// <summary>
        /// Copies the collection objects to a one-dimensional <see cref="T:System.Array">Array</see> instance beginning at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the values copied from the collection.</param>
        /// <param name="index">The index of the array at which to begin inserting.</param>
        public void CopyTo(SearchContentModuleInfo[] array, int index)
        {
            this.List.CopyTo(array, index);
        }

        /// <summary>
        /// Creates a one-dimensional <see cref="T:System.Array">Array</see> instance containing the collection items.
        /// </summary>
        /// <returns>Array of type SearchContentModuleInfo.</returns>
        public SearchContentModuleInfo[] ToArray()
        {
            var arr = new SearchContentModuleInfo[this.Count];
            this.CopyTo(arr, 0);
            return arr;
        }
    }
#pragma warning restore 0618
}
