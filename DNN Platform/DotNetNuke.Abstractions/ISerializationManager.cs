// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions
{
    using System.Reflection;

    /// <summary>
    /// The Serialization Manager providesd APIs for serializing
    /// and deserializing objects.
    /// </summary>
    public interface ISerializationManager
    {
        /// <summary>
        /// Serialize the property.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="myObject">The object to serialize.</param>
        /// <param name="property">The property info.</param>
        /// <returns>A serialized string.</returns>
        string SerializeProperty<T>(T myObject, PropertyInfo property);

        /// <summary>
        /// Serialize the property.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="myObject">The object to serialize..</param>
        /// <param name="property">The property info.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A serialized string.</returns>
        string SerializeProperty<T>(T myObject, PropertyInfo property, string serializer);

        /// <summary>
        /// Deserializes the string property.
        /// </summary>
        /// <typeparam name="T">The object the string should be deserialized into.</typeparam>
        /// <param name="myObject">The object..</param>
        /// <param name="property">The property info..</param>
        /// <param name="propertyValue">The serialized string.</param>
        void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue)
            where T : class, new();

        /// <summary>
        /// Deserializes the string property.
        /// </summary>
        /// <typeparam name="T">The object the string should be deserialized into.</typeparam>
        /// <param name="myObject">The object..</param>
        /// <param name="property">The property info..</param>
        /// <param name="propertyValue">The serialized string.</param>
        /// <param name="serializer">The serializer.</param>
        void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue, string serializer)
            where T : class, new();
    }
}
