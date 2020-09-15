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
        /// Serializes the given input property.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="myObject">todo.</param>
        /// <param name="property">document.</param>
        /// <returns>document1.</returns>
        string SerializeProperty<T>(T myObject, PropertyInfo property);

        /// <summary>
        /// Needs documentation.
        /// </summary>
        /// <typeparam name="T">Needs documentation1.</typeparam>
        /// <param name="myObject">Needs documentation2.</param>
        /// <param name="property">Needs documentation3.</param>
        /// <param name="serializer">Needs documentation4.</param>
        /// <returns>Needs documentation5.</returns>
        string SerializeProperty<T>(T myObject, PropertyInfo property, string serializer);

        /// <summary>
        /// Needs documentation.
        /// </summary>
        /// <typeparam name="T">Needs documentation1.</typeparam>
        /// <param name="myObject">Needs documentation2.</param>
        /// <param name="property">Needs documentation3.</param>
        /// <param name="propertyValue">Needs documentation4.</param>
        void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue)
            where T : class, new();

        /// <summary>
        /// Needs documentation.
        /// </summary>
        /// <typeparam name="T">Needs documentation1.</typeparam>
        /// <param name="myObject">Needs documentation2.</param>
        /// <param name="property">Needs documentation3.</param>
        /// <param name="propertyValue">Needs documentation4.</param>
        /// <param name="serializer">Needs documentation5.</param>
        void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue, string serializer)
            where T : class, new();
    }
}
