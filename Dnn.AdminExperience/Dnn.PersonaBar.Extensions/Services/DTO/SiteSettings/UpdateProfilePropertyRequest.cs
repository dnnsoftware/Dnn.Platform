// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

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
