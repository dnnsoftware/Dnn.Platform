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

    public class ExportImportRepository : IExportImportRepository
    {
        private LiteDatabase _liteDb;

        public ExportImportRepository(string dbFileName)
        {
            this._liteDb = new LiteDatabase(dbFileName);
            this._liteDb.Mapper.EmptyStringToNull = false;
            this._liteDb.Mapper.TrimWhitespace = false;
        }

        ~ExportImportRepository()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public T AddSingleItem<T>(T item)
            where T : class
        {
            var collection = this.DbCollection<T>();
            collection.Insert(item);
            return item;
        }

        public T UpdateSingleItem<T>(T item)
            where T : class
        {
            var collection = this.DbCollection<T>();
            collection.Update(item);
            return item;
        }

        public T GetSingleItem<T>()
            where T : class
        {
            var collection = this.DbCollection<T>();
            var first = collection.Min();
            return collection.FindById(first);
        }

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

        public T GetItem<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
            return this.InternalGetItems(predicate).FirstOrDefault();
        }

        public IEnumerable<T> GetItems<T>(
            Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return this.InternalGetItems(predicate, orderKeySelector, asc, skip, max);
        }

        public int GetCount<T>()
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            return collection?.Count() ?? 0;
        }

        public int GetCount<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            return collection?.Count(predicate) ?? 0;
        }

        public void RebuildIndex<T>(Expression<Func<T, object>> predicate, bool unique = false)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            collection.EnsureIndex(predicate, unique);
        }

        public IEnumerable<T> GetAllItems<T>(
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return this.InternalGetItems(null, orderKeySelector, asc, skip, max);
        }

        public T GetItem<T>(int id)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            return collection.FindById(id);
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<int> idList)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => idList.Contains(p.Id);
            return this.InternalGetItems(predicate);
        }

        public IEnumerable<T> GetRelatedItems<T>(int referenceId)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => p.ReferenceId == referenceId;
            return this.InternalGetItems(predicate);
        }

        public IEnumerable<T> FindItems<T>(Expression<Func<T, bool>> predicate)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            return collection.Find(predicate);
        }

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

        public void DeleteItems<T>(Expression<Func<T, bool>> deleteExpression)
            where T : BasicExportImportDto
        {
            var collection = this.DbCollection<T>();
            if (deleteExpression != null)
            {
                collection.Delete(deleteExpression);
            }
        }

        public void CleanUpLocal(string collectionName)
        {
            if (!this._liteDb.CollectionExists(collectionName))
            {
                return;
            }

            var collection = this._liteDb.GetCollection<BsonDocument>(collectionName);
            var documentsToUpdate = collection.Find(Query.All()).ToList();
            documentsToUpdate.ForEach(x =>
            {
                x["LocalId"] = null;
            });
            collection.Update(documentsToUpdate);
        }

        private void Dispose(bool isDisposing)
        {
            var temp = Interlocked.Exchange(ref this._liteDb, null);
            temp?.Dispose();
            if (isDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        private IEnumerable<T> InternalGetItems<T>(
            Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
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

        private LiteCollection<T> DbCollection<T>()
        {
            return this._liteDb.GetCollection<T>(typeof(T).Name);
        }
    }
}
