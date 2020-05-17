// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
