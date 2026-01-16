// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System.Runtime.Serialization;

    /// <summary>A data transfer object with information about rendering an option.</summary>
    [DataContract]
    public class RenderOption
    {
        /// <summary>Gets or sets the render function.</summary>
        [DataMember(Name = "option")]
        public string Option { get; set; }
    }
}
