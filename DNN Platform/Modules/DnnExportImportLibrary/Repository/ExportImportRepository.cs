#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Dnn.ExportImport.Dto;
using Dnn.ExportImport.Interfaces;
using LiteDB;

namespace Dnn.ExportImport.Repository
{
    public class ExportImportRepository : IExportImportRepository
    {
        private LiteDatabase _liteDb;

        public ExportImportRepository(string dbFileName)
        {
            _liteDb = new LiteDatabase(dbFileName);
            _liteDb.Mapper.EmptyStringToNull = false;
            _liteDb.Mapper.TrimWhitespace = false;
        }

        ~ExportImportRepository()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            var temp = Interlocked.Exchange(ref _liteDb, null);
            temp?.Dispose();
            if (isDisposing)
                GC.SuppressFinalize(this);
        }

        public T AddSingleItem<T>(T item) where T : class
        {
            var collection = DbCollection<T>();
            collection.Insert(item);
            return item;
        }

        public T UpdateSingleItem<T>(T item) where T : class
        {
            var collection = DbCollection<T>();
            collection.Update(item);
            return item;
        }

        public T GetSingleItem<T>() where T : class
        {
            var collection = DbCollection<T>();
            var first = collection.Min();
            return collection.FindById(first);
        }

        public T CreateItem<T>(T item, int? referenceId) where T : BasicExportImportDto
        {
            if (item == null) return null;
            var collection = DbCollection<T>();
            if (referenceId != null)
            {
                item.ReferenceId = referenceId;
            }
            item.Id = collection.Insert(item);
            return item;
        }

        public void CreateItems<T>(IEnumerable<T> items, int? referenceId = null) where T : BasicExportImportDto
        {
            if (items == null) return;
            var allItems = items as List<T> ?? items.ToList();
            if (allItems.Count == 0) return;

            var collection = DbCollection<T>();
            if (referenceId != null)
            {
                allItems.ForEach(x => { x.ReferenceId = referenceId; });
            }
            collection.Insert(allItems);
        }

        public T GetItem<T>(Expression<Func<T, bool>> predicate) where T : BasicExportImportDto
        {
            return InternalGetItems(predicate).FirstOrDefault();
        }

        public IEnumerable<T> GetItems<T>(Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return InternalGetItems(predicate, orderKeySelector, asc, skip, max);
        }

        public int GetCount<T>() where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            return collection?.Count() ?? 0;
        }
        public int GetCount<T>(Expression<Func<T, bool>> predicate) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            return collection?.Count(predicate) ?? 0;
        }

        public void RebuildIndex<T>(Expression<Func<T, object>> predicate, bool unique = false) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            collection.EnsureIndex(predicate, unique);
        }

        public IEnumerable<T> GetAllItems<T>(
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            return InternalGetItems(null, orderKeySelector, asc, skip, max);
        }

        private IEnumerable<T> InternalGetItems<T>(Expression<Func<T, bool>> predicate,
            Func<T, object> orderKeySelector = null, bool asc = true, int? skip = null, int? max = null)
            where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();

            var result = predicate != null
                ? collection.Find(predicate, skip ?? 0, max ?? int.MaxValue)
                : collection.Find(Query.All(), skip ?? 0, max ?? int.MaxValue);

            if (orderKeySelector != null)
                result = asc ? result.OrderBy(orderKeySelector) : result.OrderByDescending(orderKeySelector);

            return result.AsEnumerable();
        }

        public T GetItem<T>(int id) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            return collection.FindById(id);
        }

        public IEnumerable<T> GetItems<T>(IEnumerable<int> idList)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => idList.Contains(p.Id);
            return InternalGetItems(predicate);
        }

        public IEnumerable<T> GetRelatedItems<T>(int referenceId)
            where T : BasicExportImportDto
        {
            Expression<Func<T, bool>> predicate = p => p.ReferenceId == referenceId;
            return InternalGetItems(predicate);
        }

        public IEnumerable<T> FindItems<T>(Expression<Func<T, bool>> predicate) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            return collection.Find(predicate);
        }

        public void UpdateItem<T>(T item) where T : BasicExportImportDto
        {
            if (item == null) return;
            var collection = DbCollection<T>();
            if (collection.FindById(item.Id) == null) throw new KeyNotFoundException();
            collection.Update(item);
        }

        public void UpdateItems<T>(IEnumerable<T> items) where T : BasicExportImportDto
        {
            var allItems = items as T[] ?? items.ToArray();
            if (allItems.Length == 0) return;

            var collection = DbCollection<T>();
            collection.Update(allItems);
        }

        public bool DeleteItem<T>(int id) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            var item = collection.FindById(id);
            if (item == null) throw new KeyNotFoundException();
            return collection.Delete(id);
        }

        public void DeleteItems<T>(Expression<Func<T, bool>> deleteExpression) where T : BasicExportImportDto
        {
            var collection = DbCollection<T>();
            if (deleteExpression != null)
                collection.Delete(deleteExpression);
        }

        public void CleanUpLocal(string collectionName)
        {
            if (!_liteDb.CollectionExists(collectionName)) return;
            var collection = _liteDb.GetCollection<BsonDocument>(collectionName);
            var documentsToUpdate = collection.Find(Query.All()).ToList();
            documentsToUpdate.ForEach(x =>
            {
                x["LocalId"] = null;
            });
            collection.Update(documentsToUpdate);
        }

        private LiteCollection<T> DbCollection<T>()
        {
            return _liteDb.GetCollection<T>(typeof(T).Name);
        }
    }
}
