#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System.Collections;

#endregion

namespace DotNetNuke.Services.Search
{
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
		#region "Constructors"
		
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> class.
        /// </summary>
        public SearchContentModuleInfoCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> class containing the elements of the specified source collection.
        /// </summary>
        /// <param name="value">A <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> with which to initialize the collection.</param>
        public SearchContentModuleInfoCollection(SearchContentModuleInfoCollection value)
        {
            AddRange(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> class containing the specified array of <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> objects.
        /// </summary>
        /// <param name="value">An array of <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> objects with which to initialize the collection. </param>
        public SearchContentModuleInfoCollection(SearchContentModuleInfo[] value)
        {
            AddRange(value);
        }
		
		#endregion
		
		#region "Properties"

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
                return (SearchContentModuleInfo) List[index];
            }
            set
            {
                List[index] = value;
            }
        }
		
		#endregion

		#region "Public Methods"

        /// <summary>
        /// Add an element of the specified <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to the end of the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to add to the collection.</param>
        public int Add(SearchContentModuleInfo value)
        {
            return List.Add(value);
        }

        /// <summary>
        /// Gets the index in the collection of the specified <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see>, if it exists in the collection.
        /// </summary>
        /// <param name="value">The <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> to locate in the collection.</param>
        /// <returns>The index in the collection of the specified object, if found; otherwise, -1.</returns>
        public int IndexOf(SearchContentModuleInfo value)
        {
            return List.IndexOf(value);
        }

        /// <summary>
        /// Add an element of the specified <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to the collection at the designated index.
        /// </summary>
        /// <param name="index">An <see cref="System.Int32">Integer</see> to indicate the location to add the object to the collection.</param>
        /// <param name="value">An object of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to add to the collection.</param>
        public void Insert(int index, SearchContentModuleInfo value)
        {
            List.Insert(index, value);
        }

        /// <summary>
        /// Remove the specified object of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> from the collection.
        /// </summary>
        /// <param name="value">An object of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> to remove to the collection.</param>
        public void Remove(SearchContentModuleInfo value)
        {
            List.Remove(value);
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the specified <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see>.
        /// </summary>
        /// <param name="value">The <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> to search for in the collection.</param>
        /// <returns><b>true</b> if the collection contains the specified object; otherwise, <b>false</b>.</returns>
        public bool Contains(SearchContentModuleInfo value)
        {
			//If value is not of type SearchContentModuleInfo, this will return false.
            return List.Contains(value);
        }

        /// <summary>
        /// Copies the elements of the specified <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> array to the end of the collection.
        /// </summary>
        /// <param name="value">An array of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> containing the objects to add to the collection.</param>
        public void AddRange(SearchContentModuleInfo[] value)
        {
            for (int i = 0; i <= value.Length - 1; i++)
            {
                Add(value[i]);
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
                Add((SearchContentModuleInfo) value.List[i]);
            }
        }

        /// <summary>
        /// Copies the collection objects to a one-dimensional <see cref="T:System.Array">Array</see> instance beginning at the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the values copied from the collection.</param>
        /// <param name="index">The index of the array at which to begin inserting.</param>
        public void CopyTo(SearchContentModuleInfo[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        /// Creates a one-dimensional <see cref="T:System.Array">Array</see> instance containing the collection items.
        /// </summary>
        /// <returns>Array of type SearchContentModuleInfo</returns>
        public SearchContentModuleInfo[] ToArray()
        {
            var arr = new SearchContentModuleInfo[Count];
            CopyTo(arr, 0);
            return arr;
        }
		
		#endregion
    }
    #pragma warning restore 0618
}
