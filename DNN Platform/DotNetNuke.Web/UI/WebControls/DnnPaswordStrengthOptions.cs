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

        [DataMember(Name = "validationExpression")]
        public string ValidationExpression;

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

        [DataMember(Name = "criteriaAtLeastNSpecialChars")]
        public string CriteriaAtLeastNSpecialCharsText;

        [DataMember(Name = "criteriaAtLeastNChars")]
        public string CriteriaAtLeastNCharsText;

        [DataMember(Name = "criteriaValidationExpression")]
        public string CriteriaValidationExpressionText;

		[DataMember(Name = "passwordRulesHeadText")]
		public string PasswordRulesHeadText;

        public DnnPaswordStrengthOptions()
        {
            // all the PasswordStrength related resources are located under the Website\App_GlobalResources\WebControls.resx
            MinLengthText = Utilities.GetLocalizedString("PasswordStrengthMinLength");
            WeakText = Utilities.GetLocalizedString("PasswordStrengthWeak");
            FairText = Utilities.GetLocalizedString("PasswordStrengthFair"); ;
            StrongText = Utilities.GetLocalizedString("PasswordStrengthStrong"); ;

            CriteriaAtLeastNCharsText = Utilities.GetLocalizedString("CriteriaAtLeastNChars");
            CriteriaAtLeastNSpecialCharsText = Utilities.GetLocalizedString("CriteriaAtLeastNSpecialChars");
            CriteriaValidationExpressionText = Utilities.GetLocalizedString("CriteriaValidationExpression");
            
			PasswordRulesHeadText = Utilities.GetLocalizedString("PasswordRulesHeadText");

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
            CriteriaAtLeastNSpecialCharsText = string.Format(CriteriaAtLeastNSpecialCharsText, MinNumberOfSpecialChars);

            ValidationExpression = settings.ValidationExpression;
            CriteriaValidationExpressionText = string.Format(CriteriaValidationExpressionText, ValidationExpression);
        }
    }
}
