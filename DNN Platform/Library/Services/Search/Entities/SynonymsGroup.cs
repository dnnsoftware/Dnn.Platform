﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search.Entities
{
    using System;

    [Serializable]
    public class SynonymsGroup
    {
        public int SynonymsGroupId { get; set; }

        public string SynonymsTags { get; set; }

        public int CreatedByUserId { get; set; }

        public int LastModifiedByUserId { get; set; }

        public DateTime CreatedOnDate { get; set; }

        public DateTime LastModifiedOnDate { get; set; }

        public int PortalId { get; set; }
    }
}
