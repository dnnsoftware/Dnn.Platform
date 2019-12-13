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
    public class DeleteLogSettingsRequest
    {
        public string LogTypeConfigId { get; set; }
    }
}
