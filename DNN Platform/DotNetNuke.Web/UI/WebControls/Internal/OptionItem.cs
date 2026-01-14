// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System.Runtime.Serialization;

    /// <summary>A data transfer object with information about an option in a list.</summary>
    [DataContract]
    public class OptionItem
    {
        /// <summary>Initializes a new instance of the <see cref="OptionItem"/> class.</summary>
        public OptionItem()
        {
        }

        /// <summary>Gets or sets the text.</summary>
        [DataMember(Name = "text")]
        public string Text { get; set; }

        /// <summary>Gets or sets the value.</summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }

        /// <summary>Gets or sets a value indicating whether the option is selected.</summary>
        [DataMember(Name = "selected")]
        public bool Selected { get; set; }
    }
}
