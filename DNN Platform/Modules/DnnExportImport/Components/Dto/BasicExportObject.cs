using LiteDB;

namespace Dnn.ExportImport.Components.Dto
{
    public abstract class BasicExportObject
    {
        [BsonIndex(true)]
        public int Id { get; set; }

        [BsonIndex]
        public virtual int PortalId { get; set; }

        [BsonIndex]
        public string RelationId { get; set; }

        public string CollectionName => GetType().Name.ToLowerInvariant();
    }
}
