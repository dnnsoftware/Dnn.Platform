#region Usings

using System;
using System.Xml.Serialization;

#endregion

namespace DotNetNuke.Services.EventQueue.Config
{
    [Serializable]
    public class PublishedEvent
    {
        public string EventName { get; set; }

        public string Subscribers { get; set; }
    }
}
