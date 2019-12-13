#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Tabs
{
    public class TabExistsException : TabException
    {
        public TabExistsException(int tabId, string message) : base(tabId, message)
        {            
        }

    }
}
