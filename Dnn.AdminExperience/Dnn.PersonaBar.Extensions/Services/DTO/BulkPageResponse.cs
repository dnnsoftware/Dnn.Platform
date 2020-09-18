// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class BulkPageResponse
    {
        [DataMember(Name = "overallStatus")]
        public int OverallStatus { get; set; }

        [DataMember(Name = "pages")]
        public IEnumerable<BulkPageResponseItem> Pages { get; set; }
    }
}
