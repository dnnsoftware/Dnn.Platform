#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchResultsInfoCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Represents a collection of <see cref="SearchResultsInfo">SearchResultsInfo</see> objects.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 7.1.  No longer used in the Search infrastructure.")]
    [Serializable]
    public class SearchResultsInfoCollection : CollectionBase
    {
		#region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> class.
        /// </summary>
        public SearchResultsInfoCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> class containing the elements of the specified source collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> with which to initialize the collection.</param>
        public SearchResultsInfoCollection(SearchResultsInfoCollection value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> class containing the specified array of <see cref="SearchResultsInfo">SearchResultsInfo</see> objects.
        /// </summary>
        /// <param name="value">An array of <see cref="SearchResultsInfo">SearchResultsInfo</see> objects with which to initialize the collection. </param>
        public SearchResultsInfoCollection(SearchResultsInfo[] value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> class containing the specified array of <see cref="SearchResultsInfo">SearchResultsInfo</see> objects.
        /// </summary>
        /// <param name="value">An array of <see cref="SearchResultsInfo">SearchResultsInfo</see> objects with which to initialize the collection. </param>
        public SearchResultsInfoCollection(ArrayList value)
        {
            AddRange(value);
        }

		#endregion

		#region "Properties"

        /// <summary>
        /// Gets the <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> at the specified index in the collection.
        /// <para>
        /// In VB.Net, this property is the indexer for the <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> class.
        /// </para>
        /// </summary>
        public SearchResultsInfo this[int index]
        {
            get
            {
                return (SearchResultsInfo) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

		#endregion

		#region "Public Methods"

        /// <summary>
        /// Add an element of the specified <see cref="SearchResultsInfo">SearchResultsInfo</see> to the end of the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchResultsInfo">SearchResultsInfo</see> to add to the collection.</param>
        public int Add(SearchResultsInfo value)
        {
            return List.Add(value);
        }

        /// <summary>
        /// Gets the index in the collection of the specified <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see>, if it exists in the collection.
        /// </summary>
        /// <param name="value">The <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> to locate in the collection.</param>
        /// <returns>The index in the collection of the specified object, if found; otherwise, -1.</returns>
        public int IndexOf(SearchResultsInfo value)
        {
            return List.IndexOf(value);
        }

        /// <summary>
        /// Add an element of the specified <see cref="SearchResultsInfo">SearchResultsInfo</see> to the collection at the designated index.
        /// </summary>
        /// <param name="index">An <see cref="System.Int32">Integer</see> to indicate the location to add the object to the collection.</param>
        /// <param name="value">An object of type <see cref="SearchResultsInfo">SearchResultsInfo</see> to add to the collection.</param>
        public void Insert(int index, SearchResultsInfo value)
        {
            List.Insert(index, value);
        }

        /// <summary>
        /// Remove the specified object of type <see cref="SearchResultsInfo">SearchResultsInfo</see> from the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchResultsInfo">SearchResultsInfo</see> to remove to the collection.</param>
        public void Remove(SearchResultsInfo value)
        {
            List.Remove(value);
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the specified <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see>.
        /// </summary>
        /// <param name="value">The <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> to search for in the collection.</param>
        /// <returns><b>true</b> if the collection contains the specified object; otherwise, <b>false</b>.</returns>
        public bool Contains(SearchResultsInfo value)
        {
            return List.Contains(value);
        }

        /// <summary>
        /// Copies the elements of the specified <see cref="SearchResultsInfo">SearchResultsInfo</see> array to the end of the collection.
        /// </summary>
        /// <param name="value">An array of type <see cref="SearchResultsInfo">SearchResultsInfo</see> containing the objects to add to the collection.</param>
        public void AddRange(SearchResultsInfo[] value)
        {
            for (int i = 0; i <= value.Length - 1; i++)
            {
                Add(value[i]);
            }
        }

        /// <summary>
        /// Copies the elements of the specified arraylist to the end of the collection.
        /// </summary>
        /// <param name="value">An arraylist of type <see cref="SearchResultsInfo">SearchResultsInfo</see> containing the objects to add to the collection.</param>
        public void AddRange(ArrayList value)
        {
            foreach (object obj in value)
            {
                if (obj is SearchResultsInfo)
                {
                    Add((SearchResultsInfo) obj);
                }
            }
        }

        /// <summary>
        /// Adds the contents of another <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> to the end of the collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchResultsInfoCollection">SearchResultsInfoCollection</see> containing the objects to add to the collection. </param>
        public void AddRange(SearchResultsInfoCollection value)
        {
            for (int i = 0; i <= value.Count - 1; i++)
            {
                Add((SearchResultsInfo) value.List[i]);
            }
        }

        /// <summary>
        /// Copies the collection objects to a one-dimensional <see cref="T:System.Array">Array</see> instance beginning at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the values copied from the collection.</param>
        /// <param name="index">The index of the array at which to begin inserting.</param>
        public void CopyTo(SearchResultsInfo[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        /// Creates a one-dimensional <see cref="T:System.Array">Array</see> instance containing the collection items.
        /// </summary>
        /// <returns>Array of type SearchResultsInfo</returns>
        public SearchResultsInfo[] ToArray()
        {
            var arr = new SearchResultsInfo[Count];
            CopyTo(arr, 0);
            return arr;
        }
		
		#endregion
    }
}
