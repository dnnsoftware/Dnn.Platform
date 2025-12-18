// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.Serialization;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users.Membership;

    [DataContract]
    public class DnnPaswordStrengthOptions
    {
        [DataMember(Name = "minLength")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int MinLength;

        [DataMember(Name = "minNumberOfSpecialChars")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int MinNumberOfSpecialChars;

        [DataMember(Name = "validationExpression")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ValidationExpression;

        [DataMember(Name = "minLengthText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string MinLengthText;

        [DataMember(Name = "weakText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string WeakText;

        [DataMember(Name = "fairText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FairText;

        [DataMember(Name = "strongText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string StrongText;

        [DataMember(Name = "weakColor")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string WeakColor;

        [DataMember(Name = "fairColor")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FairColor;

        [DataMember(Name = "strongColor")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string StrongColor;

        [DataMember(Name = "labelCss")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string LabelCss;

        [DataMember(Name = "meterCss")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string MeterCss;

        [DataMember(Name = "criteriaAtLeastNSpecialChars")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string CriteriaAtLeastNSpecialCharsText;

        [DataMember(Name = "criteriaAtLeastNChars")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string CriteriaAtLeastNCharsText;

        [DataMember(Name = "criteriaValidationExpression")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string CriteriaValidationExpressionText;

        [DataMember(Name = "passwordRulesHeadText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
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

        /// <summary>To avoid fetching data from the database in constructor, the OnSerializing method is consumed.</summary>
        /// <param name="context">The serialization streaming context.</param>
        [OnSerializing]
        public void OnSerializing(StreamingContext context)
        {
            int portalId = PortalController.Instance.GetCurrentSettings()?.PortalId ?? -1;
            var settings = new MembershipPasswordSettings(portalId);

            this.MinLength = settings.MinPasswordLength;
            this.CriteriaAtLeastNCharsText = string.Format(CultureInfo.CurrentCulture, this.CriteriaAtLeastNCharsText, this.MinLength);

            this.MinNumberOfSpecialChars = settings.MinNonAlphanumericCharacters;
            this.CriteriaAtLeastNSpecialCharsText = string.Format(CultureInfo.CurrentCulture, this.CriteriaAtLeastNSpecialCharsText, this.MinNumberOfSpecialChars);

            this.ValidationExpression = settings.ValidationExpression;
            this.CriteriaValidationExpressionText = string.Format(CultureInfo.CurrentCulture, this.CriteriaValidationExpressionText, this.ValidationExpression);
        }
    }
}
