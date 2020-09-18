// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    /// <summary>
    /// Implement this interface to control how property values are serialized and deserialized.
    /// </summary>
    /// <typeparam name="T">The type that will be serialized or deserialized.</typeparam>
    /// <remarks>
    /// The <see cref="ParameterAttributeBase"/> serialize property will identify a serialization
    /// class to be used for serializing and deserializing a property to be stored in the settings table.
    /// </remarks>
    public interface ISettingsSerializer<T>
    {
        /// <summary>
        /// Serialize the property value into a string.
        /// </summary>
        /// <param name="value">The value of the associated settings property.</param>
        /// <returns>String.</returns>
        string Serialize(T value);

        /// <summary>
        /// Deserialize the property value from a string into the defined type.
        /// </summary>
        /// <param name="value">The serialized value of the associated settings property.</param>
        /// <returns>An object of the specified type.</returns>
        T Deserialize(string value);
    }
}
