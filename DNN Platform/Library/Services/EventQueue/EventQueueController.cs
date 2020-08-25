// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.EventQueue.Config;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>
    /// EventQueueController provides business layer of event queue.
    /// </summary>
    /// <remarks>
    /// Sometimes when your module running in DotNetNuke,and contains some operats didn't want to execute immediately.
    /// e.g: after your module installed into system, and some component you want to registed when the application restart,
    /// you can send an 'Application_Start' message, so your specific operation will be executed when application has been restart.
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// var oAppStartMessage = new EventMessage
    /// {
    ///     Sender = sender,
    ///     Priority = MessagePriority.High,
    ///     ExpirationDate = DateTime.Now.AddYears(-1),
    ///     SentDate = DateTime.Now,
    ///     Body = "",
    ///     ProcessorType = "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
    ///     ProcessorCommand = "UpdateSupportedFeatures"
    /// };
    /// oAppStartMessage.Attributes.Add("BusinessControllerClass", desktopModuleInfo.BusinessControllerClass);
    /// oAppStartMessage.Attributes.Add("DesktopModuleId", desktopModuleInfo.DesktopModuleID.ToString());
    /// EventQueueController.SendMessage(oAppStartMessage, "Application_Start");
    /// if ((forceAppRestart))
    /// {
    ///     Config.Touch();
    /// }
    /// </code>
    /// </example>
    public class EventQueueController
    {
        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns>event message collection.</returns>
        public static EventMessageCollection GetMessages(string eventName)
        {
            return FillMessageCollection(DataProvider.Instance().GetEventMessages(eventName));
        }

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        public static EventMessageCollection GetMessages(string eventName, string subscriberId)
        {
            return FillMessageCollection(DataProvider.Instance().GetEventMessagesBySubscriber(eventName, subscriberId));
        }

        /// <summary>
        /// Processes the messages.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns></returns>
        public static bool ProcessMessages(string eventName)
        {
            return ProcessMessages(GetMessages(eventName));
        }

        /// <summary>
        /// Processes the messages.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="subscriberId">The subscriber id.</param>
        /// <returns></returns>
        public static bool ProcessMessages(string eventName, string subscriberId)
        {
            return ProcessMessages(GetMessages(eventName, subscriberId));
        }

        /// <summary>
        /// Processes the messages.
        /// </summary>
        /// <param name="eventMessages">The event messages.</param>
        /// <returns></returns>
        public static bool ProcessMessages(EventMessageCollection eventMessages)
        {
            bool success = Null.NullBoolean;
            EventMessage message;
            for (int messageNo = 0; messageNo <= eventMessages.Count - 1; messageNo++)
            {
                message = eventMessages[messageNo];
                try
                {
                    object oMessageProcessor = Reflection.CreateObject(message.ProcessorType, message.ProcessorType);
                    if (!((EventMessageProcessorBase)oMessageProcessor).ProcessMessage(message))
                    {
                        throw new Exception();
                    }

                    // Set Message comlete so it is not run a second time
                    DataProvider.Instance().SetEventMessageComplete(message.EventMessageID);

                    success = true;
                }
                catch
                {
                    // log if message could not be processed
                    var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                    log.AddProperty("EventQueue.ProcessMessage", "Message Processing Failed");
                    log.AddProperty("ProcessorType", message.ProcessorType);
                    log.AddProperty("Body", message.Body);
                    log.AddProperty("Sender", message.Sender);
                    foreach (string key in message.Attributes.Keys)
                    {
                        log.AddProperty(key, message.Attributes[key]);
                    }

                    if (!string.IsNullOrEmpty(message.ExceptionMessage))
                    {
                        log.AddProperty("ExceptionMessage", message.ExceptionMessage);
                    }

                    LogController.Instance.AddLog(log);
                    if (message.ExpirationDate < DateTime.Now)
                    {
                        // Set Message comlete so it is not run a second time
                        DataProvider.Instance().SetEventMessageComplete(message.EventMessageID);
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <returns></returns>
        public static bool SendMessage(EventMessage message, string eventName)
        {
            // set the sent date if it wasn't set by the sender
            if (message.SentDate != null)
            {
                message.SentDate = DateTime.Now;
            }

            // Get the subscribers to this event
            string[] subscribers = GetSubscribers(eventName);

            // send a message for each subscriber of the specified event
            int intMessageID = Null.NullInteger;
            bool success = true;
            try
            {
                for (int indx = 0; indx <= subscribers.Length - 1; indx++)
                {
                    intMessageID = DataProvider.Instance().AddEventMessage(
                        eventName,
                        (int)message.Priority,
                        message.ProcessorType,
                        message.ProcessorCommand,
                        message.Body,
                        message.Sender,
                        subscribers[indx],
                        message.AuthorizedRoles,
                        message.ExceptionMessage,
                        message.SentDate,
                        message.ExpirationDate,
                        message.SerializeAttributes());
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
                success = Null.NullBoolean;
            }

            return success;
        }

        [Obsolete("This method is obsolete. Use Sendmessage(message, eventName) instead. Scheduled removal in v10.0.0.")]
        public bool SendMessage(EventMessage message, string eventName, bool encryptMessage)
        {
            return SendMessage(message, eventName);
        }

        private static EventMessage FillMessage(IDataReader dr, bool CheckForOpenDataReader)
        {
            EventMessage message;

            // read datareader
            bool canContinue = true;
            if (CheckForOpenDataReader)
            {
                canContinue = false;
                if (dr.Read())
                {
                    canContinue = true;
                }
            }

            if (canContinue)
            {
                message = new EventMessage();
                message.EventMessageID = Convert.ToInt32(Null.SetNull(dr["EventMessageID"], message.EventMessageID));
                message.Priority = (MessagePriority)Enum.Parse(typeof(MessagePriority), Convert.ToString(Null.SetNull(dr["Priority"], message.Priority)));
                message.ProcessorType = Convert.ToString(Null.SetNull(dr["ProcessorType"], message.ProcessorType));
                message.ProcessorCommand = Convert.ToString(Null.SetNull(dr["ProcessorCommand"], message.ProcessorCommand));
                message.Body = Convert.ToString(Null.SetNull(dr["Body"], message.Body));
                message.Sender = Convert.ToString(Null.SetNull(dr["Sender"], message.Sender));
                message.Subscribers = Convert.ToString(Null.SetNull(dr["Subscriber"], message.Subscribers));
                message.AuthorizedRoles = Convert.ToString(Null.SetNull(dr["AuthorizedRoles"], message.AuthorizedRoles));
                message.ExceptionMessage = Convert.ToString(Null.SetNull(dr["ExceptionMessage"], message.ExceptionMessage));
                message.SentDate = Convert.ToDateTime(Null.SetNull(dr["SentDate"], message.SentDate));
                message.ExpirationDate = Convert.ToDateTime(Null.SetNull(dr["ExpirationDate"], message.ExpirationDate));

                // Deserialize Attributes
                string xmlAttributes = Null.NullString;
                xmlAttributes = Convert.ToString(Null.SetNull(dr["Attributes"], xmlAttributes));
                message.DeserializeAttributes(xmlAttributes);
            }
            else
            {
                message = null;
            }

            return message;
        }

        private static EventMessageCollection FillMessageCollection(IDataReader dr)
        {
            var arr = new EventMessageCollection();
            try
            {
                EventMessage obj;
                while (dr.Read())
                {
                    // fill business object
                    obj = FillMessage(dr, false);

                    // add to collection
                    arr.Add(obj);
                }
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

            return arr;
        }

        private static string[] GetSubscribers(string eventName)
        {
            // Get the subscribers to this event
            string[] subscribers = null;
            PublishedEvent publishedEvent = null;
            if (EventQueueConfiguration.GetConfig().PublishedEvents.TryGetValue(eventName, out publishedEvent))
            {
                subscribers = publishedEvent.Subscribers.Split(";".ToCharArray());
            }
            else
            {
                subscribers = new string[] { };
            }

            return subscribers;
        }
    }
}
