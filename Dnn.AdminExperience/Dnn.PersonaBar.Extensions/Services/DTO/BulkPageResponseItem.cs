// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System.Runtime.Serialization;

    [DataContract]
    public class BulkPageResponseItem
    {
        [DataMember(Name = "pageName")]
        public string PageName { get; set; }

        [DataMember(Name = "created")]
        public int Status { get; set; }

        [DataMember(Name = "tabId")]
        public int TabId { get; set; }

        [DataMember(Name = "errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
