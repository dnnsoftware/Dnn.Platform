using System;
using System.Data;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Modules;

namespace Dnn.ExportImport.Components.Dto
{
    public abstract class BasicExportImportDto//: IHydratable
    {
        /// <summary>
        /// Id of the object in the export/import database.
        /// </summary>
        [IgnoreColumn]
        public int Id { get; set; }

        /// <summary>
        /// Referenced Id (i.e., foreighn key) for another object in the export/import database.
        /// </summary>
        [IgnoreColumn]
        public int? ReferenceId { get; set; }

        /// <summary>
        /// Id of the object in the local database. Note that this is use only during
        /// the import process and must remain NULL during the export process.
        /// </summary>
        [IgnoreColumn]
        public int? LocalId { get; set; }

//        public int KeyID { get; set; }
//
//        public abstract void Fill(IDataReader dr);
    }
}
