// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetNuke.Entities.DataStructures
{
    [Serializable]
    [DataContract]
    public class NTree<T>
    {
        public NTree()
        {
            this.Children = new List<NTree<T>>();
        }

        [DataMember(Name = "data")]
        public T Data;

        [DataMember(Name = "children")]
        public List<NTree<T>> Children;

        public bool HasChildren()
        {
            return this.Children != null && this.Children.Count > 0;
        }
    }
}
