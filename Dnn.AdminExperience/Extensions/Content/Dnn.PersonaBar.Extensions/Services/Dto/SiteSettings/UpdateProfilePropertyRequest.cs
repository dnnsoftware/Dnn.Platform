// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateProfilePropertyRequest
    {
        public int? PortalId { get; set; }

        public int? PropertyDefinitionId { get; set; }

        public string PropertyName { get; set; }

        public int DataType { get; set; }

        public string PropertyCategory { get; set; }

        public int Length { get; set; }

        public string DefaultValue { get; set; }

        public string ValidationExpression { get; set; }

        public bool Required { get; set; }

        public bool ReadOnly { get; set; }

        public bool Visible { get; set; }

        public int ViewOrder { get; set; }

        public int DefaultVisibility { get; set; }
    }
}
