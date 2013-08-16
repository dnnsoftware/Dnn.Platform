using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.UI.WebControls
{

    [DataContract]
    public class DnnDropDownListServicesOptions
    {

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
                return _parameters ?? (_parameters = new Dictionary<string, string>());
            }
        }

        [DataMember(Name = "rootId")]
        public string RootId = "Root"; // should not be (-1), as (-1) can be treated as Null.Integer

    }

    [DataContract]
    public class DnnDropDownListOptions
    {
        [DataMember(Name = "containerId")]
        public string ContainerId;

        [DataMember(Name = "internalStateFieldId")]
        public string InternalStateFieldId;

        [DataMember(Name = "selectedItemSelector")]
        public string SelectedItemSelector;

        [DataMember(Name = "itemListContainerSelector")]
        public string ItemListContainerSelector;

        [DataMember(Name = "itemListSelector")]
        public string ItemListSelector;

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

        [DataMember(Name = "loadingResultText")]
        public string LoadingResultText;

        [DataMember(Name = "selectItemDefaultText")]
        public string SelectItemDefaultText;

        [DataMember(Name = "disabled")]
        public bool Disabled = false;

        [DataMember(Name = "services")]
        public DnnDropDownListServicesOptions Services;

        [DataMember(Name = "firstItem")]
        public SerializableKeyValuePair<string, string> FirstItem;

        private List<string> _onClientSelectionChanged;

        [DataMember(Name = "onSelectionChanged")]
        public List<string> OnClientSelectionChanged
        {
            get
            {
                return _onClientSelectionChanged ?? (_onClientSelectionChanged = new List<string>());
            }
        }

        public DnnDropDownListOptions()
        {
            Services = new DnnDropDownListServicesOptions();

            // all the resources are located under the Website\App_GlobalResources\SharedResources.resx
            SortAscendingButtonTitle = Localization.GetString("DropDownList.SortAscendingButtonTitle", Localization.SharedResourceFile);
            UnsortedOrderButtonTooltip = Localization.GetString("DropDownList.UnsortedOrderButtonTooltip", Localization.SharedResourceFile);
            SortAscendingButtonTooltip = Localization.GetString("DropDownList.SortAscendingButtonTooltip", Localization.SharedResourceFile);
            SortDescendingButtonTooltip = Localization.GetString("DropDownList.SortDescendingButtonTooltip", Localization.SharedResourceFile);
            SelectedItemExpandTooltip = Localization.GetString("DropDownList.SelectedItemExpandTooltip", Localization.SharedResourceFile);
            SelectedItemCollapseTooltip = Localization.GetString("DropDownList.SelectedItemCollapseTooltip", Localization.SharedResourceFile);
            LoadingResultText = Localization.GetString("DropDownList.LoadingResultText", Localization.SharedResourceFile);
        }

    }
}
