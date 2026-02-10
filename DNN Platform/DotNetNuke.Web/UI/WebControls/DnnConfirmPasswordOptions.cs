// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>A data transfer object with information about options for the password confirmation display.</summary>
    [DataContract]
    public class DnnConfirmPasswordOptions
    {
        /// <summary>Gets or sets the first element selector.</summary>
        [DataMember(Name = "firstElementSelector")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FirstElementSelector;

        /// <summary>Gets or sets the second element selector.</summary>
        [DataMember(Name = "secondElementSelector")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string SecondElementSelector;

        /// <summary>Gets or sets the container selector.</summary>
        [DataMember(Name = "containerSelector")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ContainerSelector;

        /// <summary>Gets or sets the CSS class when the password does not match.</summary>
        [DataMember(Name = "unmatchedCssClass")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UnmatchedCssClass;

        /// <summary>Gets or sets the CSS class when the passwords match.</summary>
        [DataMember(Name = "matchedCssClass")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string MatchedCssClass;

        /// <summary>Gets or sets the CSS class when the password does not match.</summary>
        [DataMember(Name = "confirmPasswordUnmatchedText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ConfirmPasswordUnmatchedText;

        /// <summary>Gets or sets the CSS class when the passwords match.</summary>
        [DataMember(Name = "confirmPasswordMatchedText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ConfirmPasswordMatchedText;

        /// <summary>Initializes a new instance of the <see cref="DnnConfirmPasswordOptions"/> class.</summary>
        public DnnConfirmPasswordOptions()
        {
            // all the Confirm Password related resources are located under the Website\App_GlobalResources\WebControls.resx
            this.ConfirmPasswordUnmatchedText = Utilities.GetLocalizedString("ConfirmPasswordUnmatched");
            this.ConfirmPasswordMatchedText = Utilities.GetLocalizedString("ConfirmPasswordMatched");
        }
    }
}
