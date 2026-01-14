// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract]
    public class NTree<T>
    {
        [DataMember(Name = "data")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public T Data;

        [DataMember(Name = "children")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public List<NTree<T>> Children;

        /// <summary>Initializes a new instance of the <see cref="NTree{T}"/> class.</summary>
        public NTree()
        {
            this.Children = new List<NTree<T>>();
        }

        public bool HasChildren()
        {
            return this.Children != null && this.Children.Count > 0;
        }
    }
}
