using System;

namespace DotNetNuke.Entities.Tabs.Actions
{
    public class TabEventArgs : EventArgs
    {
        public TabInfo Tab { get; internal set; }
    }
}
