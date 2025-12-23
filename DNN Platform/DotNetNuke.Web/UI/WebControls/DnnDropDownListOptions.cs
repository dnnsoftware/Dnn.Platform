// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [DataContract]
    public class DnnDropDownListOptions
    {
        [DataMember(Name = "selectedItemCss")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string SelectedItemCss;

        [DataMember(Name = "internalStateFieldId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string InternalStateFieldId;

        [DataMember(Name = "disabled")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public bool Disabled;

        [DataMember(Name = "selectItemDefaultText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string SelectItemDefaultText;

        [DataMember(Name = "initialState")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public DnnDropDownListState InitialState;

        [DataMember(Name = "services")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public ItemListServicesOptions Services;

        [DataMember(Name = "itemList")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public ItemListOptions ItemList;

        private List<string> onClientSelectionChanged;

        /// <summary>Initializes a new instance of the <see cref="DnnDropDownListOptions"/> class.</summary>
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
                return this.onClientSelectionChanged ?? (this.onClientSelectionChanged = new List<string>());
            }
        }
    }
}
