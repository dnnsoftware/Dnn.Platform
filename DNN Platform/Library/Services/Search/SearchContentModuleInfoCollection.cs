// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using DotNetNuke.Collections;

    /// <summary>Represents a collection of <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> objects.</summary>
    public class SearchContentModuleInfoCollection : GenericCollectionBase<SearchContentModuleInfo>
    {
        /// <summary>Initializes a new instance of the <see cref="SearchContentModuleInfoCollection"/> class.</summary>
        public SearchContentModuleInfoCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SearchContentModuleInfoCollection"/> class containing the elements of the specified source collection.</summary>
        /// <param name="value">A <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> with which to initialize the collection.</param>
        public SearchContentModuleInfoCollection(SearchContentModuleInfoCollection value)
        {
            this.AddRange(value);
        }

        /// <summary>Initializes a new instance of the <see cref="SearchContentModuleInfoCollection"/> class containing the specified array of <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> objects.</summary>
        /// <param name="value">An array of <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> objects with which to initialize the collection. </param>
        public SearchContentModuleInfoCollection(SearchContentModuleInfo[] value)
        {
            this.AddRange(value);
        }

        /// <summary>Copies the elements of the specified <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> array to the end of the collection.</summary>
        /// <param name="value">An array of type <see cref="SearchContentModuleInfo">SearchContentModuleInfo</see> containing the objects to add to the collection.</param>
        public void AddRange(SearchContentModuleInfo[] value)
        {
            for (int i = 0; i <= value.Length - 1; i++)
            {
                this.Add(value[i]);
            }
        }

        /// <summary>Adds the contents of another <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> to the end of the collection.</summary>
        /// <param name="value">A <see cref="SearchContentModuleInfoCollection">SearchContentModuleInfoCollection</see> containing the objects to add to the collection. </param>
        public void AddRange(SearchContentModuleInfoCollection value)
        {
            for (int i = 0; i <= value.Count - 1; i++)
            {
                this.Add((SearchContentModuleInfo)value.List[i]);
            }
        }

        /// <summary>Creates a one-dimensional <see cref="System.Array">Array</see> instance containing the collection items.</summary>
        /// <returns>Array of type SearchContentModuleInfo.</returns>
        public SearchContentModuleInfo[] ToArray()
        {
            var arr = new SearchContentModuleInfo[this.Count];
            this.CopyTo(arr, 0);
            return arr;
        }
    }
}
