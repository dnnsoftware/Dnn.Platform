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