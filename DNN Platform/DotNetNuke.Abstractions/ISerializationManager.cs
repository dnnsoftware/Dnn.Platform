// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions;

using System;
using System.Reflection;

/// <summary>
/// The Serialization Manager provides APIs for serializing
/// and deserializing objects to and from strings.
/// </summary>
public interface ISerializationManager
{
    /// <summary>Serialize the value.</summary>
    /// <typeparam name="T">The type of value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The <paramref name="value" /> serialized to a <see cref="string" />.</returns>
    string SerializeValue<T>(T value);

    /// <summary>Serialize the value.</summary>
    /// <typeparam name="T">The type of value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="serializer">The name of a type implementing <c>DotNetNuke.Entities.Modules.Settings.ISettingsSerializer{T}</c> to use in serializing the value.</param>
    /// <returns>The <paramref name="value" /> serialized to a <see cref="string" />.</returns>
    string SerializeValue<T>(T value, string serializer);

    /// <summary>Serialize the property.</summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="myObject">The object to serialize.</param>
    /// <param name="property">The property info.</param>
    /// <returns>A serialized string.</returns>
    string SerializeProperty<T>(T myObject, PropertyInfo property);

    /// <summary>Serialize the property.</summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="myObject">The object to serialize..</param>
    /// <param name="property">The property info.</param>
    /// <param name="serializer">The name of a type implementing <c>DotNetNuke.Entities.Modules.Settings.ISettingsSerializer{T}</c> to use in serializing the property.</param>
    /// <returns>A serialized string.</returns>
    string SerializeProperty<T>(T myObject, PropertyInfo property, string serializer);

    /// <summary>Deserializes the string value.</summary>
    /// <typeparam name="T">The type the string should be deserialized into.</typeparam>
    /// <param name="value">The serialized string.</param>
    /// <returns>The value deserialized into the given type.</returns>
    /// <exception cref="InvalidCastException">The <paramref name="value"/> could not be deserialized into the given type.</exception>
    T DeserializeValue<T>(string value);

    /// <summary>Deserializes the string value.</summary>
    /// <typeparam name="T">The type the string should be deserialized into.</typeparam>
    /// <param name="value">The serialized string.</param>
    /// <param name="serializer">The name of a type implementing <c>DotNetNuke.Entities.Modules.Settings.ISettingsSerializer{T}</c> to use in deserializing the property.</param>
    /// <returns>The value deserialized into the given type.</returns>
    /// <exception cref="InvalidCastException">The <paramref name="value"/> could not be deserialized into the given type.</exception>
    T DeserializeValue<T>(string value, string serializer);

    /// <summary>Deserializes the string property.</summary>
    /// <typeparam name="T">The object the string should be deserialized into.</typeparam>
    /// <param name="myObject">The object.</param>
    /// <param name="property">The property info..</param>
    /// <param name="propertyValue">The serialized string.</param>
    /// <exception cref="InvalidCastException">The <paramref name="propertyValue"/> could not be deserialized into the given type.</exception>
    void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue)
        where T : class, new();

    /// <summary>Deserializes the string property.</summary>
    /// <typeparam name="T">The object the string should be deserialized into.</typeparam>
    /// <param name="myObject">The object..</param>
    /// <param name="property">The property info..</param>
    /// <param name="propertyValue">The serialized string.</param>
    /// <param name="serializer">The name of a type implementing <c>DotNetNuke.Entities.Modules.Settings.ISettingsSerializer{T}</c> to use in deserializing the property.</param>
    /// <exception cref="InvalidCastException">The <paramref name="propertyValue"/> could not be deserialized into the given type.</exception>
    void DeserializeProperty<T>(T myObject, PropertyInfo property, string propertyValue, string serializer)
        where T : class, new();
}
