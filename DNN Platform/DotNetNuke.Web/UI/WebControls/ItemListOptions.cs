// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Services.Localization;

[DataContract]
public class ItemListOptions
{
    [DataMember(Name = "sortAscendingButtonTitle")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string SortAscendingButtonTitle;

    [DataMember(Name = "unsortedOrderButtonTooltip")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UnsortedOrderButtonTooltip;

    [DataMember(Name = "sortAscendingButtonTooltip")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string SortAscendingButtonTooltip;

    [DataMember(Name = "sortDescendingButtonTooltip")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string SortDescendingButtonTooltip;

    [DataMember(Name = "selectedItemExpandTooltip")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string SelectedItemExpandTooltip;

    [DataMember(Name = "selectedItemCollapseTooltip")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string SelectedItemCollapseTooltip;

    [DataMember(Name = "searchInputPlaceHolder")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string SearchInputPlaceHolder;

    [DataMember(Name = "clearButtonTooltip")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string ClearButtonTooltip;

    [DataMember(Name = "searchButtonTooltip")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string SearchButtonTooltip;

    [DataMember(Name = "loadingResultText")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string LoadingResultText;

    [DataMember(Name = "resultsText")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string ResultsText;

    [DataMember(Name = "firstItem")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public SerializableKeyValuePair<string, string> FirstItem;

    [DataMember(Name = "disableUnspecifiedOrder")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public bool DisableUnspecifiedOrder;

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
