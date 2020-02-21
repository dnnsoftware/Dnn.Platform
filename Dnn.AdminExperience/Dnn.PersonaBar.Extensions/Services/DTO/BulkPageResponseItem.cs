// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
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
