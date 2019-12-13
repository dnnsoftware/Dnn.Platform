﻿#region Usings

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
    public class EmailLogItemsRequest
    {
        public string Subject { get; set; }

        public string Email { get; set; }

        public string Message { get; set; }

        public IEnumerable<string> LogIds { get; set; }
    }
}
