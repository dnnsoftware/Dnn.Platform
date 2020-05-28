// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Data;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.Seo.Services.Dto
{
    public class UpdateSitemapProviderRequest
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public float Priority { get; set; }

        public bool OverridePriority { get; set; }
    }
}
