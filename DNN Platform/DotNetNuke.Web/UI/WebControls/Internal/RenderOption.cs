// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls.Internal;

using System.Runtime.Serialization;

[DataContract]
public class RenderOption
{
    [DataMember(Name = "option")]
    public string Option { get; set; }
}
