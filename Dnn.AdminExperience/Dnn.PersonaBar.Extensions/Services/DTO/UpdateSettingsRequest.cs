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
    public class UpdateSettingsRequest
    {
        public string SchedulerMode { get; set; }

        public string SchedulerdelayAtAppStart { get; set; }
    }
}
