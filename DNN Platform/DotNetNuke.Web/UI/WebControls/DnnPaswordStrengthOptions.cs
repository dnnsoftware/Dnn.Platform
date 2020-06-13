// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users.Membership;

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
            this.MinLengthText = Utilities.GetLocalizedString("PasswordStrengthMinLength");
            this.WeakText = Utilities.GetLocalizedString("PasswordStrengthWeak");
            this.FairText = Utilities.GetLocalizedString("PasswordStrengthFair");
            this.StrongText = Utilities.GetLocalizedString("PasswordStrengthStrong");

            this.CriteriaAtLeastNCharsText = Utilities.GetLocalizedString("CriteriaAtLeastNChars");
            this.CriteriaAtLeastNSpecialCharsText = Utilities.GetLocalizedString("CriteriaAtLeastNSpecialChars");
            this.CriteriaValidationExpressionText = Utilities.GetLocalizedString("CriteriaValidationExpression");

            this.PasswordRulesHeadText = Utilities.GetLocalizedString("PasswordRulesHeadText");

            this.WeakColor = "#ed1e24";
            this.FairColor = "#f6d50a";
            this.StrongColor = "#69bd44";

            this.LabelCss = "min-length-text";
            this.MeterCss = "meter";
        }

        /// <summary>
        /// To avoid fetching data from the database in constructor, the OnSerializing method is consumed.
        /// </summary>
        /// <param name="context"></param>
        [OnSerializing]
        public void OnSerializing(StreamingContext context)
        {
            int portalId = PortalController.Instance.GetCurrentPortalSettings() != null ? PortalController.Instance.GetCurrentPortalSettings().PortalId : -1;
            var settings = new MembershipPasswordSettings(portalId);

            this.MinLength = settings.MinPasswordLength;
            this.CriteriaAtLeastNCharsText = string.Format(this.CriteriaAtLeastNCharsText, this.MinLength);

            this.MinNumberOfSpecialChars = settings.MinNonAlphanumericCharacters;
            this.CriteriaAtLeastNSpecialCharsText = string.Format(this.CriteriaAtLeastNSpecialCharsText, this.MinNumberOfSpecialChars);

            this.ValidationExpression = settings.ValidationExpression;
            this.CriteriaValidationExpressionText = string.Format(this.CriteriaValidationExpressionText, this.ValidationExpression);
        }
    }
}
