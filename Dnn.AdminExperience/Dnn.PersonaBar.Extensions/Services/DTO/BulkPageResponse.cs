// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class BulkPageResponse
    {
        [DataMember(Name = "overallStatus")]
        public int OverallStatus { get; set; }

        [DataMember(Name = "pages")]
        public IEnumerable<BulkPageResponseItem> Pages { get; set; }
    }
}
