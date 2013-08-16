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
            Children = new List<NTree<T>>();
        }

        [DataMember(Name = "data")]
        public T Data;

        [DataMember(Name = "children")]
        public List<NTree<T>> Children;

        public bool HasChildren()
        {
            return Children != null && Children.Count > 0;
        }

    }

}
