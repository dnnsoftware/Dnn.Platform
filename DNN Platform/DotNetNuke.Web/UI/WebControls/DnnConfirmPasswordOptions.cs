using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Web.UI.WebControls
{
    [DataContract]
    public class DnnConfirmPasswordOptions
    {
        [DataMember(Name = "firstElementSelector")]
        public string FirstElementSelector;

        [DataMember(Name = "secondElementSelector")]
        public string SecondElementSelector;

        [DataMember(Name = "containerSelector")]
        public string ContainerSelector;

        [DataMember(Name = "unmatchedCssClass")]
        public string UnmatchedCssClass;

        [DataMember(Name = "matchedCssClass")]
        public string MatchedCssClass;

        [DataMember(Name = "confirmPasswordUnmatchedText")]
        public string ConfirmPasswordUnmatchedText;

        [DataMember(Name = "confirmPasswordMatchedText")]
        public string ConfirmPasswordMatchedText;

        public DnnConfirmPasswordOptions()
        {
            // all the Confirm Password related resources are located under the Website\App_GlobalResources\WebControls.resx
            ConfirmPasswordUnmatchedText = Utilities.GetLocalizedString("ConfirmPasswordUnmatched");
            ConfirmPasswordMatchedText = Utilities.GetLocalizedString("ConfirmPasswordMatched");
        }

    }
}
