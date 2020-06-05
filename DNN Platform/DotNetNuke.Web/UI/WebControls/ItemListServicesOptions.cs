﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetNuke.Web.UI.WebControls
{
    [DataContract]
    public class ItemListServicesOptions
    {

        [DataMember(Name = "moduleId")]
        public string ModuleId = string.Empty;

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
                return this._parameters ?? (this._parameters = new Dictionary<string, string>());
            }
        }

        [DataMember(Name = "rootId")]
        public string RootId = "Root"; // should not be (-1), as (-1) can be treated as Null.Integer

    }
}
