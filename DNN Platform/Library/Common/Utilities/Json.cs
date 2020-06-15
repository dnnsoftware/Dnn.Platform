// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    using Newtonsoft.Json;

    /// <summary>
    /// Serialize or Deserialize Json.
    /// </summary>
    public static class Json
    {
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
