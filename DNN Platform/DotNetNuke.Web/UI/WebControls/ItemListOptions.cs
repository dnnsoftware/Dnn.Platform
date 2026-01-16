// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Services.Localization;

    /// <summary>A data transfer object with information about item list options.</summary>
    [DataContract]
    public class ItemListOptions
    {
        /// <summary>Gets or sets the title of the sort ascending button.</summary>
        [DataMember(Name = "sortAscendingButtonTitle")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SortAscendingButtonTitle;

        /// <summary>Gets or sets the tooltip of the unsorted order button.</summary>
        [DataMember(Name = "unsortedOrderButtonTooltip")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UnsortedOrderButtonTooltip;

        /// <summary>Gets or sets the tooltip of the sort ascending button.</summary>
        [DataMember(Name = "sortAscendingButtonTooltip")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SortAscendingButtonTooltip;

        /// <summary>Gets or sets the tooltip of the sort descending button.</summary>
        [DataMember(Name = "sortDescendingButtonTooltip")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SortDescendingButtonTooltip;

        /// <summary>Gets or sets the tooltip of the expand button.</summary>
        [DataMember(Name = "selectedItemExpandTooltip")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SelectedItemExpandTooltip;

        /// <summary>Gets or sets the tooltip of the collapse button.</summary>
        [DataMember(Name = "selectedItemCollapseTooltip")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SelectedItemCollapseTooltip;

        /// <summary>Gets or sets the placeholder of the search input.</summary>
        [DataMember(Name = "searchInputPlaceHolder")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SearchInputPlaceHolder;

        /// <summary>Gets or sets the tooltip of the clear button.</summary>
        [DataMember(Name = "clearButtonTooltip")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ClearButtonTooltip;

        /// <summary>Gets or sets the tooltip of the search button.</summary>
        [DataMember(Name = "searchButtonTooltip")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SearchButtonTooltip;

        /// <summary>Gets or sets the loading text.</summary>
        [DataMember(Name = "loadingResultText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string LoadingResultText;

        /// <summary>Gets or sets the results text.</summary>
        [DataMember(Name = "resultsText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ResultsText;

        /// <summary>Gets or sets the first item.</summary>
        [DataMember(Name = "firstItem")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public SerializableKeyValuePair<string, string> FirstItem;

        /// <summary>Gets or sets a value indicating whether to disable unspecified order.</summary>
        [DataMember(Name = "disableUnspecifiedOrder")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool DisableUnspecifiedOrder;

        /// <summary>Initializes a new instance of the <see cref="ItemListOptions"/> class.</summary>
        public ItemListOptions()
        {
            // all the resources are located under the Website\App_GlobalResources\SharedResources.resx
            this.SortAscendingButtonTitle = Localization.GetString("DropDownList.SortAscendingButtonTitle", Localization.SharedResourceFile);
            this.UnsortedOrderButtonTooltip = Localization.GetString("DropDownList.UnsortedOrderButtonTooltip", Localization.SharedResourceFile);
            this.SortAscendingButtonTooltip = Localization.GetString("DropDownList.SortAscendingButtonTooltip", Localization.SharedResourceFile);
            this.SortDescendingButtonTooltip = Localization.GetString("DropDownList.SortDescendingButtonTooltip", Localization.SharedResourceFile);
            this.SelectedItemExpandTooltip = Localization.GetString("DropDownList.SelectedItemExpandTooltip", Localization.SharedResourceFile);
            this.SelectedItemCollapseTooltip = Localization.GetString("DropDownList.SelectedItemCollapseTooltip", Localization.SharedResourceFile);
            this.SearchInputPlaceHolder = Localization.GetString("DropDownList.SearchInputPlaceHolder", Localization.SharedResourceFile);
            this.ClearButtonTooltip = Localization.GetString("DropDownList.ClearButtonTooltip", Localization.SharedResourceFile);
            this.SearchButtonTooltip = Localization.GetString("DropDownList.SearchButtonTooltip", Localization.SharedResourceFile);
            this.LoadingResultText = Localization.GetString("DropDownList.LoadingResultText", Localization.SharedResourceFile);
            this.ResultsText = Localization.GetString("DropDownList.Results", Localization.SharedResourceFile);
        }
    }
}
