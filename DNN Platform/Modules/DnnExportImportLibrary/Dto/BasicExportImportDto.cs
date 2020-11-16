// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Dto
{
    public abstract class BasicExportImportDto
    {
        /// <summary>
        /// Gets or sets id of the object in the export/import database.
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets referenced Id (i.e., foreighn key) for another object in the export/import database.
        /// </summary>
        public virtual int? ReferenceId { get; set; }

        /// <summary>
        /// Gets or sets id of the object in the local database. Note that this is use only during
        /// the import process and must remain NULL during the export process.
        /// </summary>
        public virtual int? LocalId { get; set; }
    }
}
