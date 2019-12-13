#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Dnn.PersonaBar.Security.Services.Dto
{
    public class UpdateIpFilterRequest
    {
        public string IPAddress { get; set; }

        public string SubnetMask { get; set; }

        public int RuleType { get; set; }

        public int IPFilterID { get; set; }
    }
}
