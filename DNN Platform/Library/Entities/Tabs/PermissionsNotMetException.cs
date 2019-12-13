#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Tabs
{
    public class PermissionsNotMetException : TabException
    {
        public PermissionsNotMetException(int tabId, string message) : base(tabId, message)
        {            
        }

    }
}
