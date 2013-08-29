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

        [DataMember(Name = "loadingResultText")]
        public string LoadingResultText;

        [DataMember(Name = "services")]
        public ItemListServicesOptions Services;

        [DataMember(Name = "firstItem")]
        public SerializableKeyValuePair<string, string> FirstItem;

        public ItemListOptions()
        {
            Services = new ItemListServicesOptions();

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
