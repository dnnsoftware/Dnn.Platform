// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Cache;

#endregion

namespace DotNetNuke.Services.EventQueue.Config
{
    [Serializable]
    internal class EventQueueConfiguration
    {
		#region "Constructors"
		
        internal EventQueueConfiguration()
        {
            PublishedEvents = new Dictionary<string, PublishedEvent>();
            EventQueueSubscribers = new Dictionary<string, SubscriberInfo>();
        }
		
		#endregion

		#region "Public Properties"

        internal Dictionary<string, SubscriberInfo> EventQueueSubscribers { get; set; }

        internal Dictionary<string, PublishedEvent> PublishedEvents { get; set; }
		
		#endregion

		#region "Private Methods"

        private void Deserialize(string configXml)
        {
            if (!String.IsNullOrEmpty(configXml))
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.LoadXml(configXml);
                foreach (XmlElement xmlItem in xmlDoc.SelectNodes("/EventQueueConfig/PublishedEvents/Event"))
                {
                    var oPublishedEvent = new PublishedEvent();
                    oPublishedEvent.EventName = xmlItem.SelectSingleNode("EventName").InnerText;
                    oPublishedEvent.Subscribers = xmlItem.SelectSingleNode("Subscribers").InnerText;
                    PublishedEvents.Add(oPublishedEvent.EventName, oPublishedEvent);
                }
                foreach (XmlElement xmlItem in xmlDoc.SelectNodes("/EventQueueConfig/EventQueueSubscribers/Subscriber"))
                {
                    var oSubscriberInfo = new SubscriberInfo();
                    oSubscriberInfo.ID = xmlItem.SelectSingleNode("ID").InnerText;
                    oSubscriberInfo.Name = xmlItem.SelectSingleNode("Name").InnerText;
                    oSubscriberInfo.Address = xmlItem.SelectSingleNode("Address").InnerText;
                    oSubscriberInfo.Description = xmlItem.SelectSingleNode("Description").InnerText;
                    oSubscriberInfo.PrivateKey = xmlItem.SelectSingleNode("PrivateKey").InnerText;
                    EventQueueSubscribers.Add(oSubscriberInfo.ID, oSubscriberInfo);
                }
            }
        }

        public static void RegisterEventSubscription(EventQueueConfiguration config, string eventname, SubscriberInfo subscriber)
        {
            var e = new PublishedEvent();
            e.EventName = eventname;
            e.Subscribers = subscriber.ID;
            config.PublishedEvents.Add(e.EventName, e);
            if (!config.EventQueueSubscribers.ContainsKey(subscriber.ID))
            {
                config.EventQueueSubscribers.Add(subscriber.ID, subscriber);
            }
        }

        private string Serialize()
        {
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.Indent = true;
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = false;

            var sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("EventQueueConfig");

                writer.WriteStartElement("PublishedEvents");
                foreach (string key in PublishedEvents.Keys)
                {
                    writer.WriteStartElement("Event");

                    writer.WriteElementString("EventName", PublishedEvents[key].EventName);
                    writer.WriteElementString("Subscribers", PublishedEvents[key].Subscribers);

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("EventQueueSubscribers");
                foreach (string key in EventQueueSubscribers.Keys)
                {
                    writer.WriteStartElement("Subscriber");

                    writer.WriteElementString("ID", EventQueueSubscribers[key].ID);
                    writer.WriteElementString("Name", EventQueueSubscribers[key].Name);
                    writer.WriteElementString("Address", EventQueueSubscribers[key].Address);
                    writer.WriteElementString("Description", EventQueueSubscribers[key].Description);
                    writer.WriteElementString("PrivateKey", EventQueueSubscribers[key].PrivateKey);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                //Close EventQueueConfig
                writer.WriteEndElement();

                writer.Close();
                return sb.ToString();
            }
        }
		#endregion
        internal static EventQueueConfiguration GetConfig()
        {
            var config = (EventQueueConfiguration) DataCache.GetCache("EventQueueConfig");
            if ((config == null))
            {
                string filePath = Globals.HostMapPath + "EventQueue\\EventQueue.config";
                if (File.Exists(filePath))
                {
                    config = new EventQueueConfiguration();
                    //Deserialize into EventQueueConfiguration
                    config.Deserialize(FileSystemUtils.ReadFile(filePath));
                    //Set back into Cache
                    DataCache.SetCache("EventQueueConfig", config, new DNNCacheDependency(filePath));
                }
                else
                {
					//make a default config file
                    config = new EventQueueConfiguration();
                    config.PublishedEvents = new Dictionary<string, PublishedEvent>();
                    config.EventQueueSubscribers = new Dictionary<string, SubscriberInfo>();
                    var subscriber = new SubscriberInfo("DNN Core");
                    RegisterEventSubscription(config, "Application_Start", subscriber);
                    RegisterEventSubscription(config, "Application_Start_FirstRequest", subscriber);
                    SaveConfig(config, filePath);
                }
            }
            return config;
        }

        internal static void SaveConfig(EventQueueConfiguration config, string filePath)
        {
            StreamWriter oStream = File.CreateText(filePath);
            oStream.WriteLine(config.Serialize());
            oStream.Close();
            //Set back into Cache
            DataCache.SetCache("EventQueueConfig", config, new DNNCacheDependency(filePath));
        }
    }
}
