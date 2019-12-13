#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Dnn.PersonaBar.TaskScheduler.Services.Dto
{
    public class UpdateSettingsRequest
    {
        public string SchedulerMode { get; set; }

        public string SchedulerdelayAtAppStart { get; set; }
    }
}
