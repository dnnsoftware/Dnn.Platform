// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System.Xml;

    public class InstalledModuleInfo
    {
        public int DesktopModuleId { get; set; }

        public int Instances { get; set; }

        public string FriendlyName { get; set; }

        public string ModuleName { get; set; }

        public string Version { get; set; }

        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("module");

            writer.WriteElementString("moduleName", this.ModuleName);
            writer.WriteElementString("friendlyName", this.FriendlyName);
            writer.WriteElementString("version", this.Version);
            writer.WriteElementString("instances", this.Instances.ToString());

            // Write end of Host Info
            writer.WriteEndElement();
        }
    }
}
