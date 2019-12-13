#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    [Serializable]
    public class LogTypeInfo
    {
        public string LogTypeCSSClass { get; set; }

        public string LogTypeDescription { get; set; }

        public string LogTypeFriendlyName { get; set; }

        public string LogTypeKey { get; set; }

        public string LogTypeOwner { get; set; }
    }
}
