// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.TaskScheduler.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
