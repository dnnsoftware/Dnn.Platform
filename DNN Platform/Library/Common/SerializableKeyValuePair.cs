// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.Common.Utilities;

    [DataContract]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        [DataMember(Name = "key")]
        public TKey Key { get; set; }

        [DataMember(Name = "value")]
        public TValue Value { get; set; }

        public override string ToString()
        {
            return Json.Serialize(this);
        }
    }
}
