// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Runtime.Serialization;

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
            this.ConfirmPasswordUnmatchedText = Utilities.GetLocalizedString("ConfirmPasswordUnmatched");
            this.ConfirmPasswordMatchedText = Utilities.GetLocalizedString("ConfirmPasswordMatched");
        }
    }
}
