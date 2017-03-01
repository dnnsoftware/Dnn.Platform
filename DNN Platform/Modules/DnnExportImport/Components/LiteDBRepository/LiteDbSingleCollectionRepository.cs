using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Users;
using DotNetNuke.Framework;
using LiteDB;

namespace Dnn.ExportImport.Components.LiteDBRepository
{
    public class LiteDbSingleCollectionRepository:  ServiceLocator<IRepository, LiteDbSingleCollectionRepository>, IRepository
    {
        
        #region Shared/Static Methods

        protected override Func<IRepository> GetFactory()
        {
            return () => new LiteDbSingleCollectionRepository();
        }

        #endregion

        public string LiteDatabase { get; set; } =$"{AssemblyDirectory}/ExportImport.db";
        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public T CreateItem<T>(T item, int portalId) where T : BasicExportObject
        {
            using (var db = new LiteDatabase(LiteDatabase))
            {
                item.PortalId = portalId;
                var collection = db.GetCollection<T>(item.CollectionName);
                item.Id = collection.Insert(item);
            }
            return item;
        }

        public IEnumerable<T> CreateItems<T>(IEnumerable<T> items, int portalId) where T : BasicExportObject
        {
            var allItems = items.ToList();
            using (var db = new LiteDatabase(LiteDatabase))
            {
                foreach (var item in allItems)
                {
                    item.PortalId = portalId;
                    var collection = db.GetCollection<T>(item.CollectionName);
                    item.Id = collection.Insert(item);
                }
            }
            return allItems;
        }

        public T GetItem<T>(Expression<Func<T, bool>> predicate, string collectionName, int portalId) where T : BasicExportObject
        {
            throw new NotImplementedException();
        }

        public LiteCollection<T> GetAllItems<T>(string collectionName, int portalId) where T : BasicExportObject
        {
            using (var db = new LiteDatabase(LiteDatabase))
            {
                var items = db.GetCollection<T>(collectionName);
                return items;
            }
        }

        public T GetItem<T>(string collectionName, int id, int portalId) where T : BasicExportObject
        {
            throw new NotImplementedException();
        }

        public LiteCollection<T> GetItems<T>(string collectionName, IEnumerable<string> relationIds, int portalId, int? top = null) where T : BasicExportObject
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
