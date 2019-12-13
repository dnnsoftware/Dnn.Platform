#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Tabs
{
    public class TabException : Exception
    {
        public TabException(int tabId, string message) : base(message)
        {
            TabId = tabId;
        }

        public int TabId { get; private set; }
    }
}
