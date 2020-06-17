﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Seo.Services.Dto
{
    using System;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class UpdateExtensionUrlProviderStatusRequest
    {
        public int ProviderId { get; set; }

        public bool IsActive { get; set; }
    }
}
