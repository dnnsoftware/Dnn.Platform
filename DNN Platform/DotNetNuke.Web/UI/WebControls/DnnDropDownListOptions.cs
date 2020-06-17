// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

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

        [DataMember(Name = "services")]
        public ItemListServicesOptions Services;

        [DataMember(Name = "itemList")]
        public ItemListOptions ItemList;

        private List<string> _onClientSelectionChanged;

        public DnnDropDownListOptions()
        {
            this.SelectedItemCss = "selected-item";
            this.SelectItemDefaultText = string.Empty;
            this.Services = new ItemListServicesOptions();
            this.ItemList = new ItemListOptions();
        }

        [DataMember(Name = "onSelectionChanged")]
        public List<string> OnClientSelectionChanged
        {
            get
            {
                return this._onClientSelectionChanged ?? (this._onClientSelectionChanged = new List<string>());
            }
        }
    }
}
