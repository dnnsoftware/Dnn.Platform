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

namespace Dnn.PersonaBar.CssEditor.Services.Dto
{
    public class UpdateCssRequest
    {
        public int PortalId { get; set; }

        public string StyleSheetContent { get; set; }
    }
}
