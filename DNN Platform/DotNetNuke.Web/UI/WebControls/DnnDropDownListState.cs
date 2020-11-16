// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Runtime.Serialization;
    using DotNetNuke.Common;

    [DataContract]
    public class DnnDropDownListState
    {
        [DataMember(Name = "selectedItem")]
        public SerializableKeyValuePair<string, string> SelectedItem;
    }
}
