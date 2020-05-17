// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Xml;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public class InstalledModuleInfo
    {
        public int DesktopModuleId { get; set; }

        public int Instances { get; set; }

        public string FriendlyName { get; set; }

        public string ModuleName { get; set; }

        public string Version { get; set; }

        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst 
            writer.WriteStartElement("module");

            writer.WriteElementString("moduleName", ModuleName);
            writer.WriteElementString("friendlyName", FriendlyName);
            writer.WriteElementString("version", Version);
            writer.WriteElementString("instances", Instances.ToString());

            //Write end of Host Info 
            writer.WriteEndElement();
        }
    }
}
