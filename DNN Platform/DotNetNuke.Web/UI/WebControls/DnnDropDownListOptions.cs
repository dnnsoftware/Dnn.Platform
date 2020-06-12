// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetNuke.Web.UI.WebControls
{
    [DataContract]
    public class DnnDropDownListOptions
    {

        [DataMember(Name = "selectedItemCss")]
        public string SelectedItemCss;

        [DataMember(Name = "internalStateFieldId")]
        public string InternalStateFieldId;

        [DataMember(Name = "disabled")]
        public bool Disabled = false;

        [DataMember(Name = "selectItemDefaultText")]
        public string SelectItemDefaultText;

        [DataMember(Name = "initialState")]
        public DnnDropDownListState InitialState;

        private List<string> _onClientSelectionChanged;

        [DataMember(Name = "onSelectionChanged")]
        public List<string> OnClientSelectionChanged
        {
            get
            {
                return this._onClientSelectionChanged ?? (this._onClientSelectionChanged = new List<string>());
            }
        }

        [DataMember(Name = "services")]
        public ItemListServicesOptions Services;

        [DataMember(Name = "itemList")]
        public ItemListOptions ItemList;

        public DnnDropDownListOptions()
        {
            this.SelectedItemCss = "selected-item";
            this.SelectItemDefaultText = string.Empty;
            this.Services = new ItemListServicesOptions();
            this.ItemList = new ItemListOptions();
        }

    }
}
