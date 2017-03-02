namespace Dnn.ExportImport.Components.Dto
{
    public abstract class BasicExportImportDto
    {
        /// <summary>
        /// Id of the object in the export/import database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Referenced Id (i.e., foreighn key) for another object in the export/import database.
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// Id of the object in the local database. Note that this is use only during
        /// the import process and must remain NULL during the export process.
        /// </summary>
        public int? LocalId { get; set; }
    }
}
