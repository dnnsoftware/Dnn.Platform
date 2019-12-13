// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// <summary>
    /// Serialize or Deserialize Json
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
