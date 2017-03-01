using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Interfaces;
using LiteDB;

namespace Dnn.ExportImport.Components.Repository
{
    public class ExportImportRepository : IExportImportRepository
    {
        private LiteDatabase _lightDb;

        public ExportImportRepository(string dbFileName)
        {
            _lightDb = new LiteDatabase(dbFileName);
        }

        ~ExportImportRepository()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            var temp = Interlocked.Exchange(ref _lightDb, null);
            temp?.Dispose();
        }

        public T CreateItem<T>(T item, int? referenceId) where T : BasicExportImportDto
        {
            item.ReferenceId = referenceId;
            var collection = _lightDb.GetCollection<T>(item.CollectionName);
            item.Id = collection.Insert(item);
            collection.EnsureIndex(x => x.ReferenceId);
            return item;
        }

        public IEnumerable<T> CreateItems<T>(IEnumerable<T> items, int? referenceId) where T : BasicExportImportDto
        {
            var allItems = items.ToList();
            foreach (var item in allItems)
            {
                item.ReferenceId = referenceId;
                var collection = _lightDb.GetCollection<T>(item.CollectionName);
                item.Id = collection.Insert(item);
            }
            return allItems;
        }

        public T GetItem<T>(Expression<Func<T, bool>> predicate, string collectionName) where T : BasicExportImportDto
        {
            return InternalGetItems(predicate, collectionName).FirstOrDefault();
        }

        public IEnumerable<T> GetItems<T>(Expression<Func<T, bool>> predicate, string collectionName,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return InternalGetItems(predicate, collectionName, orderKeySelector, asc, skip, max);
        }

        public IEnumerable<T> GetAllItems<T>(string collectionName,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return InternalGetItems(null, collectionName, orderKeySelector, asc, skip, max);
        }

        private IEnumerable<T> InternalGetItems<T>(Expression<Func<T, bool>> predicate, string collectionName,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            var collection = _lightDb.GetCollection<T>(collectionName);
            var result = predicate != null ? collection.Find(predicate) : collection.FindAll();

            if (orderKeySelector != null)
                result = asc ? result.OrderBy(orderKeySelector) : result.OrderByDescending(orderKeySelector);
            else
                result = result.OrderBy(x => x.Id);

            if (skip != null)
                result = result.Skip(skip.Value);

            if (max != null)
                result = result.Take(max.Value);

            return result.AsEnumerable();
        }

        public T GetItem<T>(int id, string collectionName) where T : BasicExportImportDto
        {
            var collection = _lightDb.GetCollection<T>(collectionName);
            return collection.FindById(id);
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<int> idList, string collectionName)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => idList.Contains(p.Id);
            return InternalGetItems(predicate, collectionName);
        }

        public IEnumerable<T> GetRelatedItems<T>(int referenceId, string collectionName)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => p.ReferenceId == referenceId;
            return InternalGetItems(predicate, collectionName);
        }

        public T UpdateItem<T>(int id, T item, int? referenceId) where T : BasicExportImportDto
        {
            var collection = _lightDb.GetCollection<T>(item.CollectionName);

            if (collection.FindById(id) == null) throw new KeyNotFoundException();

            item.ReferenceId = referenceId;
            item.Id = id;
            collection.Update(item);
            collection.EnsureIndex(x => x.ReferenceId);
            return item;
        }

        public bool DeleteItem<T>(int id, string collectionName) where T : BasicExportImportDto
        {
            var collection = _lightDb.GetCollection<T>(collectionName);
            var item = collection.FindById(id);
            if (item == null) throw new KeyNotFoundException();
            return collection.Delete(id);
        }
    }
}
