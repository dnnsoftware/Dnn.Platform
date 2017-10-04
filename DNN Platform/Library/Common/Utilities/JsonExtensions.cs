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
using System.Web.Script.Serialization;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// <summary>
    ///   Json Extensions based on the JavaScript Serializer in System.web
    /// </summary>
    public static class JsonExtensionsWeb
    {
        private static JavaScriptSerializer SerializerFactory()
        {
            // Allow large JSON strings to be serialized and deserialized.
            return new JavaScriptSerializer {MaxJsonLength = Int32.MaxValue};
        }

        /// <summary>
        ///   Serializes a type to Json. Note the type must be marked Serializable 
        ///   or include a DataContract attribute.
        /// </summary>
        /// <param name = "value"></param>
        /// <returns></returns>
        public static string ToJsonString(object value)
        {
            var ser = SerializerFactory();
            string json = ser.Serialize(value);
            return json;
        }

        /// <summary>
        ///   Extension method on object that serializes the value to Json. 
        ///   Note the type must be marked Serializable or include a DataContract attribute.
        /// </summary>
        /// <param name = "value"></param>
        /// <returns></returns>
        public static string ToJson(this object value)
        {
            return ToJsonString(value);
        }

        /// <summary>
        ///   Deserializes a json string into a specific type. 
        ///   Note that the type specified must be serializable.
        /// </summary>
        /// <param name = "json"></param>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static object FromJsonString(string json, Type type)
        {
            // *** Have to use Reflection with a 'dynamic' non constant type instance 
            var ser = SerializerFactory();

            object result = ser.GetType().GetMethod("Deserialize").MakeGenericMethod(type).Invoke(ser, new object[1] {json});
            return result;
        }

        /// <summary>
        ///   Extension method to string that deserializes a json string 
        ///   into a specific type. 
        ///   Note that the type specified must be serializable.
        /// </summary>
        /// <param name = "json"></param>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static object FromJson(this string json, Type type)
        {
            return FromJsonString(json, type);
        }

        public static TType FromJson<TType>(this string json)
        {
            var ser = SerializerFactory();

            var result = ser.Deserialize<TType>(json);
            return result;
        }
    }
}
