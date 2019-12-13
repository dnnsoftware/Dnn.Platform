#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Dnn.PersonaBar.AdminLogs.Services.Dto
{
    public class UpdateLogSettingsRequest
    {
        public string ID { get; set; }

        public bool LoggingIsActive { get; set; }

        public string LogTypeKey { get; set; }

        public string LogTypePortalID { get; set; }

        public string KeepMostRecent { get; set; }

        public bool EmailNotificationIsActive { get; set; }

        public int NotificationThreshold { get; set; }

        public int NotificationThresholdTime { get; set; }

        public int NotificationThresholdTimeType { get; set; }

        public string MailFromAddress { get; set; }

        public string MailToAddress { get; set; }
    }
}
