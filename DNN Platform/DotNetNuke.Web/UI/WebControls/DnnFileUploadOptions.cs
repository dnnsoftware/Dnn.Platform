using System.Runtime.Serialization;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.UI.WebControls
{
    [DataContract]
    public class DnnFileUploadOptions
    {

        [DataMember(Name = "folderPicker")]
        public DnnDropDownListOptions FolderPicker;

        public DnnFileUploadOptions()
        {
            FolderPicker = new DnnDropDownListOptions();
            // all the resources are located under the Website\App_GlobalResources\SharedResources.resx
            //Title = Localization.GetString("GettingStarted.Title", Localization.SharedResourceFile);
        }

    }
}
