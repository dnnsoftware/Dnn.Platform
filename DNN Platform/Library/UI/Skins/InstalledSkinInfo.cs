// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System.Xml;

    public class InstalledSkinInfo
    {
        public bool InUse { get; set; }

        public string SkinFile { get; set; }

        public string SkinName { get; set; }

        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("skin");

            writer.WriteElementString("skinName", this.SkinName);
            writer.WriteElementString("inUse", this.InUse.ToString());

            // Write end of Host Info
            writer.WriteEndElement();
        }
    }
}
