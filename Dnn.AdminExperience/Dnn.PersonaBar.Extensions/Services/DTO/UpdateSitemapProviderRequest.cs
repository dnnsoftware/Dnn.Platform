// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
