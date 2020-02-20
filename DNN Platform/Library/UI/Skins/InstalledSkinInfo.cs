// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Xml;

#endregion

namespace DotNetNuke.UI.Skins
{
    public class InstalledSkinInfo
    {
        public bool InUse { get; set; }

        public string SkinFile { get; set; }

        public string SkinName { get; set; }

        public void WriteXml(XmlWriter writer)
        {
			//Write start of main elemenst 
            writer.WriteStartElement("skin");

            writer.WriteElementString("skinName", SkinName);
            writer.WriteElementString("inUse", InUse.ToString());

            //Write end of Host Info 
            writer.WriteEndElement();
        }
    }
}
