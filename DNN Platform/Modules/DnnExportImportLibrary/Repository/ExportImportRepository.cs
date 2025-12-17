// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;

    using Dnn.ExportImport.Dto;
    using Dnn.ExportImport.Interfaces;
    using LiteDB;

    /// <inheritdoc/>
    public class ExportImportRepository : IExportImportRepository
    {
        private LiteDatabase liteDb;

        /// <summary>Initializes a new instance of the <see cref="ExportImportRepository"/> class.</summary>
        /// <param name="dbFileName">The LiteDB connection string.</param>
        public ExportImportRepository(string dbFileName)
        {
            this.liteDb = new LiteDatabase(new ConnectionString(dbFileName) { Upgrade = true });
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
            GC.SuppressFinalize(this);
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var temp = Interlocked.Exchange(ref this.liteDb, null);
                temp?.Dispose();
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
