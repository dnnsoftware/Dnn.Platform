﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.ExportImport.Dto
{
    public abstract class BasicExportImportDto
    {
        /// <summary>
        /// Id of the object in the export/import database.
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Referenced Id (i.e., foreighn key) for another object in the export/import database.
        /// </summary>
        public virtual int? ReferenceId { get; set; }

        /// <summary>
        /// Id of the object in the local database. Note that this is use only during
        /// the import process and must remain NULL during the export process.
        /// </summary>
        public virtual int? LocalId { get; set; }
    }
}
