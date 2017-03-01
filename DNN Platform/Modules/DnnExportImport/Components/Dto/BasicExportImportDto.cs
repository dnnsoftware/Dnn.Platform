namespace Dnn.ExportImport.Components.Dto
{
    public abstract class BasicExportImportDto
    {
        public int Id { get; set; }

        public int? ReferenceId { get; set; }

        public string CollectionName => GetType().Name.ToLowerInvariant();
    }
}
