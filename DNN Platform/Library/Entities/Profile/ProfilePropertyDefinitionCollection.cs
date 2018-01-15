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

using System;
using System.Collections;

#endregion

namespace DotNetNuke.Entities.Profile
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Profile
    /// Class:      ProfilePropertyDefinitionCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfilePropertyDefinitionCollection class provides Business Layer methods for 
    /// a collection of property Definitions
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ProfilePropertyDefinitionCollection : CollectionBase
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new default collection
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new Collection from an ArrayList of ProfilePropertyDefinition objects
        /// </summary>
        /// <param name="definitionsList">An ArrayList of ProfilePropertyDefinition objects</param>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection(ArrayList definitionsList)
        {
            AddRange(definitionsList);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new Collection from a ProfilePropertyDefinitionCollection
        /// </summary>
        /// <param name="collection">A ProfilePropertyDefinitionCollection</param>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection(ProfilePropertyDefinitionCollection collection)
        {
            AddRange(collection);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets an item in the collection.
        /// </summary>
        /// <remarks>This overload returns the item by its index. </remarks>
        /// <param name="index">The index to get</param>
        /// <returns>A ProfilePropertyDefinition object</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition this[int index]
        {
            get
            {
                return (ProfilePropertyDefinition) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an item in the collection.
        /// </summary>
        /// <remarks>This overload returns the item by its name</remarks>
        /// <param name="name">The name of the Property to get</param>
        /// <returns>A ProfilePropertyDefinition object</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a property Definition to the collectio.
        /// </summary>
        /// <param name="value">A ProfilePropertyDefinition object</param>
        /// <returns>The index of the property Definition in the collection</returns>
        /// -----------------------------------------------------------------------------
        public int Add(ProfilePropertyDefinition value)
        {
            return List.Add(value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Add an ArrayList of ProfilePropertyDefinition objects
        /// </summary>
        /// <param name="definitionsList">An ArrayList of ProfilePropertyDefinition objects</param>
        /// -----------------------------------------------------------------------------
        public void AddRange(ArrayList definitionsList)
        {
            foreach (ProfilePropertyDefinition objProfilePropertyDefinition in definitionsList)
            {
                Add(objProfilePropertyDefinition);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Add an existing ProfilePropertyDefinitionCollection
        /// </summary>
        /// <param name="collection">A ProfilePropertyDefinitionCollection</param>
        /// -----------------------------------------------------------------------------
        public void AddRange(ProfilePropertyDefinitionCollection collection)
        {
            foreach (ProfilePropertyDefinition objProfilePropertyDefinition in collection)
            {
                Add(objProfilePropertyDefinition);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the collection contains a property definition
        /// </summary>
        /// <param name="value">A ProfilePropertyDefinition object</param>
        /// <returns>A Boolean True/False</returns>
        /// -----------------------------------------------------------------------------
        public bool Contains(ProfilePropertyDefinition value)
        {
            return List.Contains(value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a sub-collection of items in the collection by category.
        /// </summary>
        /// <param name="category">The category to get</param>
        /// <returns>A ProfilePropertyDefinitionCollection object</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection GetByCategory(string category)
        {
            var collection = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition profileProperty in InnerList)
            {
                if (profileProperty.PropertyCategory == category)
                {
                    collection.Add(profileProperty);
                }
            }
            return collection;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an item in the collection by Id.
        /// </summary>
        /// <param name="id">The id of the Property to get</param>
        /// <returns>A ProfilePropertyDefinition object</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition GetById(int id)
        {
            ProfilePropertyDefinition profileItem = null;
            foreach (ProfilePropertyDefinition profileProperty in InnerList)
            {
                if (profileProperty.PropertyDefinitionId == id)
                {
                    profileItem = profileProperty;
                }
            }
            return profileItem;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an item in the collection by name.
        /// </summary>
        /// <param name="name">The name of the Property to get</param>
        /// <returns>A ProfilePropertyDefinition object</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition GetByName(string name)
        {
            ProfilePropertyDefinition profileItem = null;
            foreach (ProfilePropertyDefinition profileProperty in InnerList)
            {
                if (profileProperty?.PropertyName == name)
                {
					//Found Profile property
                    profileItem = profileProperty;
                }
            }
            return profileItem;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the index of a property Definition
        /// </summary>
        /// <param name="value">A ProfilePropertyDefinition object</param>
        /// <returns>The index of the property Definition in the collection</returns>
        /// -----------------------------------------------------------------------------
        public int IndexOf(ProfilePropertyDefinition value)
        {
            return List.IndexOf(value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Inserts a property Definition into the collectio.
        /// </summary>
        /// <param name="value">A ProfilePropertyDefinition object</param>
        /// <param name="index">The index to insert the item at</param>
        /// -----------------------------------------------------------------------------
        public void Insert(int index, ProfilePropertyDefinition value)
        {
            List.Insert(index, value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Removes a property definition from the collection
        /// </summary>
        /// <param name="value">The ProfilePropertyDefinition object to remove</param>
        /// -----------------------------------------------------------------------------
        public void Remove(ProfilePropertyDefinition value)
        {
            List.Remove(value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sorts the collection using the ProfilePropertyDefinitionComparer (ie by ViewOrder)
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void Sort()
        {
            InnerList.Sort(new ProfilePropertyDefinitionComparer());
        }
    }
}
