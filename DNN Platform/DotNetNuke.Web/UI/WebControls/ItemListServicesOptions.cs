using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetNuke.Web.UI.WebControls
{
    [DataContract]
    public class ItemListServicesOptions
    {

        [DataMember(Name = "serviceRoot")]
        public string ServiceRoot;

        [DataMember(Name = "getTreeMethod")]
        public string GetTreeMethod;

        [DataMember(Name = "sortTreeMethod")]
        public string SortTreeMethod;

        [DataMember(Name = "getNodeDescendantsMethod")]
        public string GetNodeDescendantsMethod;

        [DataMember(Name = "searchTreeMethod")]
        public string SearchTreeMethod;

        [DataMember(Name = "getTreeWithNodeMethod")]
        public string GetTreeWithNodeMethod;

        private Dictionary<string, string> _parameters;

        [DataMember(Name = "parameters")]
        public Dictionary<string, string> Parameters
        {
            get
            {
                return _parameters ?? (_parameters = new Dictionary<string, string>());
            }
        }

        [DataMember(Name = "rootId")]
        public string RootId = "Root"; // should not be (-1), as (-1) can be treated as Null.Integer

    }
}
