// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateProfilePropertyOrdersRequest
    {
        public int? PortalId { get; set; }

        public UpdateProfilePropertyRequest[] Properties { get; set; }
    }
}
