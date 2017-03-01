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
            if (temp != null)
            {
                _lightDb.Dispose();
            }
        }

        public T CreateItem<T>(T item, int portalId) where T : BasicExportObject
        {
            item.PortalId = portalId;
            var collection = _lightDb.GetCollection<T>(item.CollectionName);
            item.Id = collection.Insert(item);
            return item;
        }

        public IEnumerable<T> CreateItems<T>(IEnumerable<T> items, int portalId) where T : BasicExportObject
        {
            var allItems = items.ToList();
            foreach (var item in allItems)
            {
                item.PortalId = portalId;
                var collection = _lightDb.GetCollection<T>(item.CollectionName);
                item.Id = collection.Insert(item);
            }
            return allItems;
        }

        public T GetItem<T>(Expression<Func<T, bool>> predicate, string collectionName, int portalId) where T : BasicExportObject
        {
            throw new NotImplementedException();
        }

        public LiteCollection<T> GetAllItems<T>(string collectionName, int portalId) where T : BasicExportObject
        {
            var items = _lightDb.GetCollection<T>(collectionName);
            return items;
        }

        public T GetItem<T>(string collectionName, int id, int portalId) where T : BasicExportObject
        {
            throw new NotImplementedException();
        }

        public LiteCollection<T> GetItems<T>(string collectionName, IEnumerable<string> relationIds, int portalId, int? top = null)
            where T : BasicExportObject
        {
            throw new NotImplementedException();
        }

        public T UpdateItem<T>(int id, T item, int portalId) where T : BasicExportObject
        {
            throw new NotImplementedException();
        }

        public void DeleteItem<T>(int id, string collectionName, int portalId) where T : BasicExportObject
        {
            throw new NotImplementedException();
        }
    }
}
