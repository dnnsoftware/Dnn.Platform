﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
                return _onClientSelectionChanged ?? (_onClientSelectionChanged = new List<string>());
            }
        }

        [DataMember(Name = "services")]
        public ItemListServicesOptions Services;

        [DataMember(Name = "itemList")]
        public ItemListOptions ItemList;

        public DnnDropDownListOptions()
        {
            SelectedItemCss = "selected-item";
            SelectItemDefaultText = "";
            Services = new ItemListServicesOptions();
            ItemList = new ItemListOptions();
        }

    }
}
