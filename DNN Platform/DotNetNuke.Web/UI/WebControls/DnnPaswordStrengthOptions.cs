using System;
using System.Runtime.Serialization;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users.Membership;

namespace DotNetNuke.Web.UI.WebControls
{
    [DataContract]
    public class DnnPaswordStrengthOptions
    {
        [DataMember(Name = "minLength")]
        public int MinLength;

        [DataMember(Name = "minNumberOfSpecialChars")]
        public int MinNumberOfSpecialChars;

        [DataMember(Name = "minLengthText")]
        public string MinLengthText;

        [DataMember(Name = "weakText")]
        public string WeakText;

        [DataMember(Name = "fairText")]
        public string FairText;

        [DataMember(Name = "strongText")]
        public string StrongText;

        [DataMember(Name = "weakColor")]
        public string WeakColor;

        [DataMember(Name = "fairColor")]
        public string FairColor;

        [DataMember(Name = "strongColor")]
        public string StrongColor;

        [DataMember(Name = "labelCss")]
        public string LabelCss;

        [DataMember(Name = "meterCss")]
        public string MeterCss;

        [DataMember(Name = "criteriaOneUpperCaseLetterText")]
        public string CriteriaOneUpperCaseLetterText;

        [DataMember(Name = "criteriaOneLowerCaseLetterText")]
        public string CriteriaOneLowerCaseLetterText;

        [DataMember(Name = "criteriaSpecialCharText")]
        public string CriteriaSpecialCharText;

        [DataMember(Name = "criteriaOneNumberText")]
        public string CriteriaOneNumberText;

        [DataMember(Name = "criteriaAtLeastNCharsText")]
        public string CriteriaAtLeastNCharsText;

        public DnnPaswordStrengthOptions()
        {
            // all the PasswordStrength related resources are located under the Website\App_GlobalResources\WebControls.resx
            MinLengthText = Utilities.GetLocalizedString("PasswordStrengthMinLength");
            WeakText = Utilities.GetLocalizedString("PasswordStrengthWeak");
            FairText = Utilities.GetLocalizedString("PasswordStrengthFair"); ;
            StrongText = Utilities.GetLocalizedString("PasswordStrengthStrong"); ;

            CriteriaOneUpperCaseLetterText = Utilities.GetLocalizedString("CriteriaOneUpperCaseLetter");
            CriteriaOneLowerCaseLetterText = Utilities.GetLocalizedString("CriteriaOneLowerCaseLetter");
            CriteriaOneNumberText = Utilities.GetLocalizedString("CriteriaOneNumber");
            CriteriaAtLeastNCharsText = Utilities.GetLocalizedString("CriteriaAtLeastNChars");

            WeakColor = "#ed1e24";
            FairColor = "#f6d50a";
            StrongColor = "#69bd44";

            LabelCss = "min-length-text";
            MeterCss = "meter";
        }

        /// <summary>
        /// To avoid fetching data from the database in constructor, the OnSerializing method is consumed
        /// </summary>
        /// <param name="context"></param>
        [OnSerializing]
        public void OnSerializing(StreamingContext context)
        {
            int portalId = (PortalController.Instance.GetCurrentPortalSettings()) != null ? (PortalController.Instance.GetCurrentPortalSettings().PortalId) : -1;
            var settings = new MembershipPasswordSettings(portalId);
            MinLength = settings.MinPasswordLength;
            CriteriaAtLeastNCharsText = string.Format(CriteriaAtLeastNCharsText, MinLength);

            MinNumberOfSpecialChars = settings.MinNonAlphanumericCharacters;
            CriteriaSpecialCharText = MinNumberOfSpecialChars > 0 ?
                string.Format(Utilities.GetLocalizedString("CriteriaAtLeastNSpecialChars"), MinNumberOfSpecialChars) :
                Utilities.GetLocalizedString("CriteriaSpecialChar");
        }
    }
}
