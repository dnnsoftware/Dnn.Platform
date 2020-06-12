// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Xml.Serialization;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Web.DDRMenu
{
    [Serializable]
    [XmlRoot("xmlroot", Namespace = "")]
    public class MenuXml
    {
// ReSharper disable InconsistentNaming
        public MenuNode root { get; set; }
        public UserInfo user { get; set; }

// ReSharper restore InconsistentNaming
    }
}
