
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information


using System;
using System.Collections;

namespace DotNetNuke.Services.Personalization
{
    [Serializable]
    public class PersonalizationInfo
    {
        public int UserId { get; set; }

        public int PortalId { get; set; }

        public bool IsModified { get; set; }

        public Hashtable Profile { get; set; }
    }
}
