// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace Dnn.ExportImport.Dto.Users
{
    public class ExportUserProfile : BasicExportImportDto
    {
        public int ProfileId { get; set; }

        public int UserId { get; set; }

        public int PropertyDefinitionId { get; set; }

        public string PropertyName { get; set; }

        public string PropertyValue { get; set; }
        public string PropertyText { get; set; }
        public int Visibility { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string ExtendedVisibility { get; set; }
    }
}
