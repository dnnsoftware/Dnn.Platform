﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.UI.WebControls
{
    [DataContract]
    public class ItemListOptions
    {

        [DataMember(Name = "sortAscendingButtonTitle")]
        public string SortAscendingButtonTitle;

        [DataMember(Name = "unsortedOrderButtonTooltip")]
        public string UnsortedOrderButtonTooltip;

        [DataMember(Name = "sortAscendingButtonTooltip")]
        public string SortAscendingButtonTooltip;

        [DataMember(Name = "sortDescendingButtonTooltip")]
        public string SortDescendingButtonTooltip;

        [DataMember(Name = "selectedItemExpandTooltip")]
        public string SelectedItemExpandTooltip;

        [DataMember(Name = "selectedItemCollapseTooltip")]
        public string SelectedItemCollapseTooltip;

        [DataMember(Name = "searchInputPlaceHolder")]
        public string SearchInputPlaceHolder;

        [DataMember(Name = "clearButtonTooltip")]
        public string ClearButtonTooltip;

        [DataMember(Name = "searchButtonTooltip")]
        public string SearchButtonTooltip;

        [DataMember(Name = "loadingResultText")]
        public string LoadingResultText;

        [DataMember(Name = "resultsText")]
        public string ResultsText;

        [DataMember(Name = "firstItem")]
        public SerializableKeyValuePair<string, string> FirstItem;

        [DataMember(Name = "disableUnspecifiedOrder")]
        public bool DisableUnspecifiedOrder;

        public ItemListOptions()
        {
            // all the resources are located under the Website\App_GlobalResources\SharedResources.resx
            SortAscendingButtonTitle = Localization.GetString("DropDownList.SortAscendingButtonTitle", Localization.SharedResourceFile);
            UnsortedOrderButtonTooltip = Localization.GetString("DropDownList.UnsortedOrderButtonTooltip", Localization.SharedResourceFile);
            SortAscendingButtonTooltip = Localization.GetString("DropDownList.SortAscendingButtonTooltip", Localization.SharedResourceFile);
            SortDescendingButtonTooltip = Localization.GetString("DropDownList.SortDescendingButtonTooltip", Localization.SharedResourceFile);
            SelectedItemExpandTooltip = Localization.GetString("DropDownList.SelectedItemExpandTooltip", Localization.SharedResourceFile);
            SelectedItemCollapseTooltip = Localization.GetString("DropDownList.SelectedItemCollapseTooltip", Localization.SharedResourceFile);
            SearchInputPlaceHolder = Localization.GetString("DropDownList.SearchInputPlaceHolder", Localization.SharedResourceFile);
            ClearButtonTooltip = Localization.GetString("DropDownList.ClearButtonTooltip", Localization.SharedResourceFile);
            SearchButtonTooltip = Localization.GetString("DropDownList.SearchButtonTooltip", Localization.SharedResourceFile);
            LoadingResultText = Localization.GetString("DropDownList.LoadingResultText", Localization.SharedResourceFile);
            ResultsText = Localization.GetString("DropDownList.Results", Localization.SharedResourceFile);
        }

    }
}
