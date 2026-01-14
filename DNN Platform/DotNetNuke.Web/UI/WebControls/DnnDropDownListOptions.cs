// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>A data transfer object with information about dropdown list options.</summary>
    [DataContract]
    public class DnnDropDownListOptions
    {
        /// <summary>Gets or sets the selected item CSS.</summary>
        [DataMember(Name = "selectedItemCss")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SelectedItemCss;

        /// <summary>Gets or sets the field ID of the internal state.</summary>
        [DataMember(Name = "internalStateFieldId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string InternalStateFieldId;

        /// <summary>Gets or sets a value indicating whether the dropdown list is disabled.</summary>
        [DataMember(Name = "disabled")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool Disabled;

        /// <summary>Gets or sets the default text for the selected item.</summary>
        [DataMember(Name = "selectItemDefaultText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SelectItemDefaultText;

        /// <summary>Gets or sets the initial state.</summary>
        [DataMember(Name = "initialState")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public DnnDropDownListState InitialState;

        /// <summary>Gets or sets the services.</summary>
        [DataMember(Name = "services")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public ItemListServicesOptions Services;

        /// <summary>Gets or sets the item list.</summary>
        [DataMember(Name = "itemList")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
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

        /// <summary>Gets register a list of JavaScript methods that are executed when the selection from the list control changes on the client.</summary>
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
