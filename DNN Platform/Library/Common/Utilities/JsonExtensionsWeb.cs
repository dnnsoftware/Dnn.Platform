﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Web.Script.Serialization;

    /// <summary>JSON Extensions based on the JavaScript Serializer in System.web.</summary>
    public static class JsonExtensionsWeb
    {
        /// <summary>Serializes a type to JSON. Note the type must be marked Serializable or include a DataContract attribute.</summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The JSON string.</returns>
        public static string ToJsonString(object value)
        {
            var ser = SerializerFactory();
            string json = ser.Serialize(value);
            return json;
        }

        /// <summary>Extension method on object that serializes the value to JSON. Note the type must be marked Serializable or include a DataContract attribute.</summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The JSON string.</returns>
        public static string ToJson(this object value)
        {
            return ToJsonString(value);
        }

        /// <summary>Deserializes a JSON string into a specific type. Note that the type specified must be serializable.</summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="type">The type to deserialize the JSON into.</param>
        /// <returns>The deserialized value.</returns>
        public static object FromJsonString(string json, Type type)
        {
            // *** Have to use Reflection with a 'dynamic' non constant type instance
            var ser = SerializerFactory();

            object result = ser.GetType().GetMethod("Deserialize").MakeGenericMethod(type).Invoke(ser, new object[1] { json });
            return result;
        }

        /// <summary>Extension method to string that deserializes a JSON string into a specific type. Note that the type specified must be serializable.</summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="type">The type to deserialize the JSON into.</param>
        /// <returns>The deserialized value.</returns>
        public static object FromJson(this string json, Type type)
        {
            return FromJsonString(json, type);
        }

        /// <summary>Extension method to string that deserializes a JSON string into a specific type. Note that the type specified must be serializable.</summary>
        /// <typeparam name="TType">The type to deserialize the JSON into.</typeparam>
        /// <param name="json">A JSON string.</param>
        /// <returns>The deserialized value.</returns>
        public static TType FromJson<TType>(this string json)
        {
            var ser = SerializerFactory();

            var result = ser.Deserialize<TType>(json);
            return result;
        }

        private static JavaScriptSerializer SerializerFactory()
        {
            // Allow large JSON strings to be serialized and deserialized.
            return new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        }
    }
}
