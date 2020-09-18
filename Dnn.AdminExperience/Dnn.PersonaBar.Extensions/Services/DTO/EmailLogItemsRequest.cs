// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.AdminLogs.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class EmailLogItemsRequest
    {
        public string Subject { get; set; }

        public string Email { get; set; }

        public string Message { get; set; }

        public IEnumerable<string> LogIds { get; set; }
    }
}
