// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

using DotNetNuke.Entities.Users;

[Serializable]
[XmlRoot("xmlroot", Namespace = "")]
public class MenuXml
{
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public MenuNode root { get; set; }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public UserInfo user { get; set; }
}
