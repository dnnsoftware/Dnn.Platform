// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Collections;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.DataStore
    /// Class:      SearchCriteria
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Represents a collection of <see cref="SearchCriteria">SearchCriteria</see> objects.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.. Scheduled removal in v10.0.0.")]
    public class SearchCriteriaCollection : CollectionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCriteriaCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> class.
        /// </summary>
        public SearchCriteriaCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCriteriaCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> class containing the elements of the specified source collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> with which to initialize the collection.</param>
        public SearchCriteriaCollection(SearchCriteriaCollection value)
        {
            this.AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCriteriaCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> class containing the specified array of <see cref="SearchCriteria">SearchCriteria</see> objects.
        /// </summary>
        /// <param name="value">An array of <see cref="SearchCriteria">SearchCriteria</see> objects with which to initialize the collection. </param>
        public SearchCriteriaCollection(SearchCriteria[] value)
        {
            this.AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCriteriaCollection"/> class.
        /// Initializes a new instance of the <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> class containing the elements of the specified source collection.
        /// </summary>
        /// <param name="value">A criteria string with which to initialize the collection.</param>
        public SearchCriteriaCollection(string value)
        {
            // split search criteria into words
            string[] words = value.Split(' ');

            // Add all criteria without modifiers
            foreach (string word in words)
            {
                var criterion = new SearchCriteria();
                if ((!word.StartsWith("+")) && (!word.StartsWith("-")))
                {
                    criterion.MustInclude = false;
                    criterion.MustExclude = false;
                    criterion.Criteria = word;
                    this.Add(criterion);
                }
            }

            // Add all mandatory criteria
            foreach (string word in words)
            {
                var criterion = new SearchCriteria();
                if (word.StartsWith("+"))
                {
                    criterion.MustInclude = true;
                    criterion.MustExclude = false;
                    criterion.Criteria = word.Remove(0, 1);
                    this.Add(criterion);
                }
            }

            // Add all excluded criteria
            foreach (string word in words)
            {
                var criterion = new SearchCriteria();
                if (word.StartsWith("-"))
                {
                    criterion.MustInclude = false;
                    criterion.MustExclude = true;
                    criterion.Criteria = word.Remove(0, 1);
                    this.Add(criterion);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> at the specified index in the collection.
        /// <para>
        /// In VB.Net, this property is the indexer for the <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> class.
        /// </para>
        /// </summary>
        public SearchCriteria this[int index]
        {
            get
            {
                return (SearchCriteria)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        /// <summary>
        /// Add an element of the specified <see cref="SearchCriteria">SearchCriteria</see> to the end of the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchCriteria">SearchCriteria</see> to add to the collection.</param>
        /// <returns></returns>
        public int Add(SearchCriteria value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Gets the index in the collection of the specified <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see>, if it exists in the collection.
        /// </summary>
        /// <param name="value">The <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> to locate in the collection.</param>
        /// <returns>The index in the collection of the specified object, if found; otherwise, -1.</returns>
        public int IndexOf(SearchCriteria value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Add an element of the specified <see cref="SearchCriteria">SearchCriteria</see> to the collection at the designated index.
        /// </summary>
        /// <param name="index">An <see cref="int">Integer</see> to indicate the location to add the object to the collection.</param>
        /// <param name="value">An object of type <see cref="SearchCriteria">SearchCriteria</see> to add to the collection.</param>
        public void Insert(int index, SearchCriteria value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Remove the specified object of type <see cref="SearchCriteria">SearchCriteria</see> from the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchCriteria">SearchCriteria</see> to remove to the collection.</param>
        public void Remove(SearchCriteria value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the specified <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see>.
        /// </summary>
        /// <param name="value">The <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> to search for in the collection.</param>
        /// <returns><b>true</b> if the collection contains the specified object; otherwise, <b>false</b>.</returns>
        public bool Contains(SearchCriteria value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Copies the elements of the specified <see cref="SearchCriteria">SearchCriteria</see> array to the end of the collection.
        /// </summary>
        /// <param name="value">An array of type <see cref="SearchCriteria">SearchCriteria</see> containing the objects to add to the collection.</param>
        public void AddRange(SearchCriteria[] value)
        {
            for (int i = 0; i <= value.Length - 1; i++)
            {
                this.Add(value[i]);
            }
        }

        /// <summary>
        /// Adds the contents of another <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> to the end of the collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchCriteriaCollection">SearchCriteriaCollection</see> containing the objects to add to the collection. </param>
        public void AddRange(SearchCriteriaCollection value)
        {
            for (int i = 0; i <= value.Count - 1; i++)
            {
                this.Add((SearchCriteria)value.List[i]);
            }
        }

        /// <summary>
        /// Copies the collection objects to a one-dimensional <see cref="T:System.Array">Array</see> instance beginning at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the values copied from the collection.</param>
        /// <param name="index">The index of the array at which to begin inserting.</param>
        public void CopyTo(SearchCriteria[] array, int index)
        {
            this.List.CopyTo(array, index);
        }

        /// <summary>
        /// Creates a one-dimensional <see cref="T:System.Array">Array</see> instance containing the collection items.
        /// </summary>
        /// <returns>Array of type SearchCriteria.</returns>
        public SearchCriteria[] ToArray()
        {
            var arr = new SearchCriteria[this.Count];
            this.CopyTo(arr, 0);
            return arr;
        }
    }
}
