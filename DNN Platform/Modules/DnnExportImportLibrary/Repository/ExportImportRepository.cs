// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Repository
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;

    using Dnn.ExportImport.Dto;
    using Dnn.ExportImport.Interfaces;
    using LiteDB;

    /// <inheritdoc/>
    public class ExportImportRepository : IExportImportRepository
    {
        // Legacy (LiteDB v3.x/v4.x, on-disk "v7") files carry this ASCII signature starting at byte
        // offset 25, with the file-format version stored at byte offset 52. This mirrors the internal
        // LiteDB.Engine.FileReaderV7.IsVersion check and lets us detect a legacy export database without
        // opening it through the broken upgrade/rebuild path.
        private const string LegacyFileSignature = "** This is a LiteDB file **";
        private const int LegacySignatureOffset = 25;
        private const int LegacyVersionOffset = 52;
        private const byte LegacyFileVersion = 7;

        private LiteDatabase liteDb;
        private string migratedDbFileName;

        /// <summary>Initializes a new instance of the <see cref="ExportImportRepository"/> class.</summary>
        /// <param name="dbFileName">The LiteDB connection string.</param>
        public ExportImportRepository(string dbFileName)
        {
            // A DNN 9.10.2-era (LiteDB 3.x on-disk format) export database is upgraded/rebuilt in place
            // when opened with Upgrade = true. LiteDB 5.0.21 has a bug in that rebuild path
            // (IndexService.FindAll loop guard) that throws "Detected loop in FindAll({0})" for any
            // collection larger than ~2,550 records, blocking the import before it starts. To avoid the
            // broken rebuild, detect a legacy-format file and migrate it into a fresh LiteDB 5.x database
            // by streaming documents through normal inserts (which do not use the broken guard), then open
            // the migrated copy. Native 5.x databases (and small legacy ones that still fail to migrate)
            // fall back to the original Upgrade = true fast path with no behavior change.
            var fileToOpen = dbFileName;
            if (IsLegacyFormatFile(dbFileName))
            {
                fileToOpen = this.TryMigrateLegacyDatabase(dbFileName) ?? dbFileName;
            }

            this.liteDb = new LiteDatabase(new ConnectionString(fileToOpen) { Upgrade = true });
            this.liteDb.Mapper.EmptyStringToNull = false;
            this.liteDb.Mapper.TrimWhitespace = false;
        }

        /// <summary>Finalizes an instance of the <see cref="ExportImportRepository"/> class.</summary>
        ~ExportImportRepository()
        {
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <inheritdoc/>
        public T AddSingleItem<T>(T item)
            where T : class
        {
            var collection = this.DbCollection<T>();
            collection.Insert(item);
            return item;
        }

        /// <inheritdoc/>
        public T UpdateSingleItem<T>(T item)
            where T : class
        {
            var collection = this.DbCollection<T>();
            collection.Update(item);
            return item;
        }

        /// <inheritdoc/>
        public T GetSingleItem<T>()
            where T : class
        {
            var collection = this.DbCollection<T>();
            var first = collection.Min();
            return collection.FindById(first);
        }

        /// <inheritdoc/>
        public T CreateItem<T>(T item, int? referenceId)
            where T : BasicExportImportDto
        {
            if (item == null)
            {
                return null;
            }

            var collection = this.DbCollection<T>();
            if (referenceId != null)
            {
                item.ReferenceId = referenceId;
            }

            item.Id = collection.Insert(item);
            return item;
        }

        /// <inheritdoc/>
        public void CreateItems<T>(IEnumerable<T> items, int? referenceId = null)
            where T : BasicExportImportDto
        {
            if (items == null)
            {
                return;
            }

            var allItems = items as List<T> ?? items.ToList();
            if (allItems.Count == 0)
            {
                return;
            }

            var collection = this.DbCollection<T>();
            if (referenceId != null)
            {
                allItems.ForEach(x => { x.ReferenceId = referenceId; });
            }

            collection.Insert(allItems);
        }

        /// <inheritdoc/>
        public T GetItem<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
            return this.InternalGetItems(predicate).FirstOrDefault();
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetItems<T>(
            Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null,
            bool asc = true,
            int? skip = null,
            int? max = null)
            where T : BasicExportImportDto
        {
            return this.InternalGetItems(predicate, orderKeySelector, asc, skip, max);
        }

        /// <inheritdoc/>
        public int GetCount<T>()
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            return collection?.Count() ?? 0;
        }

        /// <inheritdoc/>
        public int GetCount<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            return collection?.Count(predicate) ?? 0;
        }

        /// <inheritdoc/>
        public void RebuildIndex<T>(Expression<Func<T, object>> predicate, bool unique = false)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            collection.EnsureIndex(predicate, unique);
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetAllItems<T>(
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return this.InternalGetItems(null, orderKeySelector, asc, skip, max);
        }

        /// <inheritdoc/>
        public T GetItem<T>(int id)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            return collection.FindById(id);
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetItems<T>(IEnumerable<int> idList)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => idList.Contains(p.Id);
            return this.InternalGetItems(predicate);
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetRelatedItems<T>(int referenceId)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => p.ReferenceId == referenceId;
            return this.InternalGetItems(predicate);
        }

        /// <inheritdoc/>
        public IEnumerable<T> FindItems<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            return collection.Find(predicate);
        }

        /// <inheritdoc/>
        public void UpdateItem<T>(T item)
            where T : BasicExportImportDto
        {
            if (item == null)
            {
                return;
            }

            var collection = this.DbCollection<T>();
            if (collection.FindById(item.Id) == null)
            {
                throw new KeyNotFoundException();
            }

            collection.Update(item);
        }

        /// <inheritdoc/>
        public void UpdateItems<T>(IEnumerable<T> items)
            where T : BasicExportImportDto
        {
            var allItems = items as T[] ?? items.ToArray();
            if (allItems.Length == 0)
            {
                return;
            }

            var collection = this.DbCollection<T>();
            collection.Update(allItems);
        }

        /// <inheritdoc/>
        public bool DeleteItem<T>(int id)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            var item = collection.FindById(id);
            if (item == null)
            {
                throw new KeyNotFoundException();
            }

            return collection.Delete(id);
        }

        /// <inheritdoc/>
        public void DeleteItems<T>(Expression<Func<T, bool>> deleteExpression)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            if (deleteExpression != null)
            {
                collection.DeleteMany(deleteExpression);
            }
        }

        /// <inheritdoc/>
        public void CleanUpLocal(string collectionName)
        {
            if (!this.liteDb.CollectionExists(collectionName))
            {
                return;
            }

            var collection = this.liteDb.GetCollection<BsonDocument>(collectionName);
            var documentsToUpdate = collection.Find(Query.All()).ToList();
            documentsToUpdate.ForEach(x =>
            {
                x["LocalId"] = null;
            });
            collection.Update(documentsToUpdate);
        }

        /// <summary>
        /// Determines whether the given export database file was written by a legacy (LiteDB v3.x/v4.x,
        /// on-disk "v7") DNN version, prior to the in-place migration this constructor performs.
        /// Callers can use this to warn about a cross-version import before opening the repository,
        /// since opening it migrates a legacy file in place and this signature no longer applies afterward.
        /// </summary>
        /// <param name="dbFileName">The database file path.</param>
        /// <returns><c>true</c> when the file exists and carries the legacy LiteDB file signature and version.</returns>
        public static bool IsLegacyExportFile(string dbFileName)
        {
            return IsLegacyFormatFile(dbFileName);
        }

        /// <summary>Determines whether the given file is a legacy (LiteDB v3.x/v4.x, on-disk "v7") database.</summary>
        /// <param name="dbFileName">The database file path.</param>
        /// <returns><c>true</c> when the file exists and carries the legacy LiteDB file signature and version.</returns>
        private static bool IsLegacyFormatFile(string dbFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(dbFileName) || !File.Exists(dbFileName))
                {
                    return false;
                }

                var header = new byte[LegacyVersionOffset + 1];
                using (var stream = new FileStream(dbFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var read = stream.Read(header, 0, header.Length);
                    if (read < header.Length)
                    {
                        return false;
                    }
                }

                var signature = System.Text.Encoding.UTF8.GetString(header, LegacySignatureOffset, LegacyFileSignature.Length);
                return signature == LegacyFileSignature && header[LegacyVersionOffset] == LegacyFileVersion;
            }
            catch
            {
                // If the header cannot be read for any reason, treat the file as non-legacy and let the
                // normal open path handle (and report) any problem.
                return false;
            }
        }

        /// <summary>
        /// Reads every collection and document from a legacy-format LiteDB database using LiteDB's own
        /// legacy reader (<c>FileReaderV7</c>) and writes them into a fresh LiteDB 5.x database via normal
        /// inserts, sidestepping the broken in-place rebuild. Documents are inserted with their original
        /// <c>_id</c> values preserved.
        /// </summary>
        /// <param name="sourceDbFileName">The legacy database file to migrate.</param>
        /// <returns>The path to the migrated 5.x database, or <c>null</c> if migration was not possible.</returns>
        private string TryMigrateLegacyDatabase(string sourceDbFileName)
        {
            var targetDbFileName = sourceDbFileName + ".migrated";
            try
            {
                if (File.Exists(targetDbFileName))
                {
                    File.Delete(targetDbFileName);
                }

                var liteDbAssembly = typeof(LiteDatabase).Assembly;
                var engineSettingsType = liteDbAssembly.GetType("LiteDB.Engine.EngineSettings");
                var fileReaderType = liteDbAssembly.GetType("LiteDB.Engine.FileReaderV7");
                if (engineSettingsType == null || fileReaderType == null)
                {
                    return null;
                }

                var engineSettings = Activator.CreateInstance(engineSettingsType);
                engineSettingsType.GetProperty("Filename").SetValue(engineSettings, sourceDbFileName);

                var reader = (IDisposable)Activator.CreateInstance(
                    fileReaderType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { engineSettings },
                    null);

                using (reader)
                {
                    fileReaderType.GetMethod("Open").Invoke(reader, null);
                    var getCollections = fileReaderType.GetMethod("GetCollections");
                    var getDocuments = fileReaderType.GetMethod("GetDocuments");

                    using (var targetDb = new LiteDatabase(new ConnectionString(targetDbFileName)))
                    {
                        var collectionNames = (IEnumerable<string>)getCollections.Invoke(reader, null);
                        foreach (var collectionName in collectionNames.ToList())
                        {
                            var documents = (IEnumerable<BsonDocument>)getDocuments.Invoke(reader, new object[] { collectionName });
                            var target = targetDb.GetCollection<BsonDocument>(collectionName);

                            // Insert in batches to bound memory while streaming large collections.
                            foreach (var batch in Batch(documents, 2000))
                            {
                                target.Insert(batch);
                            }
                        }

                        targetDb.Checkpoint();
                    }
                }

                this.migratedDbFileName = targetDbFileName;
                return targetDbFileName;
            }
            catch
            {
                // If anything about the legacy migration fails, discard the partial copy and fall back to
                // the original open path so behavior is never worse than before this fix.
                this.SafeDeleteMigratedFile(targetDbFileName);
                this.migratedDbFileName = null;
                return null;
            }
        }

        private static IEnumerable<IList<BsonDocument>> Batch(IEnumerable<BsonDocument> source, int size)
        {
            var bucket = new List<BsonDocument>(size);
            foreach (var item in source)
            {
                bucket.Add(item);
                if (bucket.Count == size)
                {
                    yield return bucket;
                    bucket = new List<BsonDocument>(size);
                }
            }

            if (bucket.Count > 0)
            {
                yield return bucket;
            }
        }

        private void SafeDeleteMigratedFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best-effort cleanup of the temporary migrated copy; ignore failures.
            }
        }

        private void Dispose(bool isDisposing)
        {
            var temp = Interlocked.Exchange(ref this.liteDb, null);
            temp?.Dispose();

            var migrated = Interlocked.Exchange(ref this.migratedDbFileName, null);
            this.SafeDeleteMigratedFile(migrated);

            if (isDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        private IEnumerable<T> InternalGetItems<T>(
            Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null,
            bool asc = true,
            int? skip = null,
            int? max = null)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();

            var result = predicate != null
                ? collection.Find(predicate, skip ?? 0, max ?? int.MaxValue)
                : collection.Find(Query.All(), skip ?? 0, max ?? int.MaxValue);

            if (orderKeySelector != null)
            {
                result = asc ? result.OrderBy(orderKeySelector) : result.OrderByDescending(orderKeySelector);
            }

            return result.AsEnumerable();
        }

        private ILiteCollection<T> DbCollection<T>()
        {
            return this.liteDb.GetCollection<T>(typeof(T).Name);
        }
    }
}
