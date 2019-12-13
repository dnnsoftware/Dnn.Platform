using System;
using System.Xml;

namespace DotNetNuke.Entities.Tabs.Actions
{
    public class TabSyncEventArgs : TabEventArgs
    {
        public XmlNode TabNode { get; set; }
    }
}
