// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
    public class ScheduleDto
    {
        public int ScheduleID { get; set; }

        public string TypeFullName { get; set; }

        public string FriendlyName { get; set; }

        public int TimeLapse { get; set; }

        public string TimeLapseMeasurement { get; set; }

        public int RetryTimeLapse { get; set; }

        public string RetryTimeLapseMeasurement { get; set; }

        public int RetainHistoryNum { get; set; }

        public string AttachToEvent { get; set; }

        public bool CatchUpEnabled { get; set; }

        public bool Enabled { get; set; }

        public string ObjectDependencies { get; set; }

        public string ScheduleStartDate { get; set; }

        public string Servers { get; set; }
    }
}
