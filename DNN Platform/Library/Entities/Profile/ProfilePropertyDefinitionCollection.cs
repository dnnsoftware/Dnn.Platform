// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Profile
{
    using System;
    using System.Collections;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Profile
    /// Class:      ProfilePropertyDefinitionCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfilePropertyDefinitionCollection class provides Business Layer methods for
    /// a collection of property Definitions.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ProfilePropertyDefinitionCollection : CollectionBase
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class.
        /// Constructs a new default collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class.
        /// Constructs a new Collection from an ArrayList of ProfilePropertyDefinition objects.
        /// </summary>
        /// <param name="definitionsList">An ArrayList of ProfilePropertyDefinition objects.</param>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection(ArrayList definitionsList)
        {
            this.AddRange(definitionsList);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilePropertyDefinitionCollection"/> class.
        /// Constructs a new Collection from a ProfilePropertyDefinitionCollection.
        /// </summary>
        /// <param name="collection">A ProfilePropertyDefinitionCollection.</param>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection(ProfilePropertyDefinitionCollection collection)
        {
            this.AddRange(collection);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an item in the collection.
        /// </summary>
        /// <remarks>This overload returns the item by its name.</remarks>
        /// <param name="name">The name of the Property to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition this[string name]
        {
            get
            {
                return this.GetByName(name);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets an item in the collection.
        /// </summary>
        /// <remarks>This overload returns the item by its index. </remarks>
        /// <param name="index">The index to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition this[int index]
        {
            get
            {
                return (ProfilePropertyDefinition)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a property Definition to the collectio.
        /// </summary>
        /// <param name="value">A ProfilePropertyDefinition object.</param>
        /// <returns>The index of the property Definition in the collection.</returns>
        /// -----------------------------------------------------------------------------
        public int Add(ProfilePropertyDefinition value)
        {
            return this.List.Add(value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Add an ArrayList of ProfilePropertyDefinition objects.
        /// </summary>
        /// <param name="definitionsList">An ArrayList of ProfilePropertyDefinition objects.</param>
        /// -----------------------------------------------------------------------------
        public void AddRange(ArrayList definitionsList)
        {
            foreach (ProfilePropertyDefinition objProfilePropertyDefinition in definitionsList)
            {
                this.Add(objProfilePropertyDefinition);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Add an existing ProfilePropertyDefinitionCollection.
        /// </summary>
        /// <param name="collection">A ProfilePropertyDefinitionCollection.</param>
        /// -----------------------------------------------------------------------------
        public void AddRange(ProfilePropertyDefinitionCollection collection)
        {
            foreach (ProfilePropertyDefinition objProfilePropertyDefinition in collection)
            {
                this.Add(objProfilePropertyDefinition);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the collection contains a property definition.
        /// </summary>
        /// <param name="value">A ProfilePropertyDefinition object.</param>
        /// <returns>A Boolean True/False.</returns>
        /// -----------------------------------------------------------------------------
        public bool Contains(ProfilePropertyDefinition value)
        {
            return this.List.Contains(value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a sub-collection of items in the collection by category.
        /// </summary>
        /// <param name="category">The category to get.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object.</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinitionCollection GetByCategory(string category)
        {
            var collection = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
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
        /// <param name="id">The id of the Property to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition GetById(int id)
        {
            ProfilePropertyDefinition profileItem = null;
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
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
        /// <param name="name">The name of the Property to get.</param>
        /// <returns>A ProfilePropertyDefinition object.</returns>
        /// -----------------------------------------------------------------------------
        public ProfilePropertyDefinition GetByName(string name)
        {
            ProfilePropertyDefinition profileItem = null;
            foreach (ProfilePropertyDefinition profileProperty in this.InnerList)
            {
                if (profileProperty?.PropertyName == name)
                {
                    // Found Profile property
                    profileItem = profileProperty;
                }
            }

            return profileItem;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the index of a property Definition.
        /// </summary>
        /// <param name="value">A ProfilePropertyDefinition object.</param>
        /// <returns>The index of the property Definition in the collection.</returns>
        /// -----------------------------------------------------------------------------
        public int IndexOf(ProfilePropertyDefinition value)
        {
            return this.List.IndexOf(value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Inserts a property Definition into the collectio.
        /// </summary>
        /// <param name="value">A ProfilePropertyDefinition object.</param>
        /// <param name="index">The index to insert the item at.</param>
        /// -----------------------------------------------------------------------------
        public void Insert(int index, ProfilePropertyDefinition value)
        {
            this.List.Insert(index, value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Removes a property definition from the collection.
        /// </summary>
        /// <param name="value">The ProfilePropertyDefinition object to remove.</param>
        /// -----------------------------------------------------------------------------
        public void Remove(ProfilePropertyDefinition value)
        {
            this.List.Remove(value);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sorts the collection using the ProfilePropertyDefinitionComparer (ie by ViewOrder).
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void Sort()
        {
            this.InnerList.Sort(new ProfilePropertyDefinitionComparer());
        }
    }
}
