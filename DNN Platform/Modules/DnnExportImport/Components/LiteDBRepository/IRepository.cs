using Dnn.ExportImport.Components.Dto;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;

namespace Dnn.ExportImport.Components.LiteDBRepository
{
    public interface IRepository : IDisposable
    {
        string LiteDatabase { get; set; }

        T CreateItem<T>(T item, int portalId) where T : BasicExportObject;
        IEnumerable<T> CreateItems<T>(IEnumerable<T> items, int portalId) where T : BasicExportObject;

        T GetItem<T>(Expression<Func<T, bool>> predicate, string collectionName, int portalId)
            where T : BasicExportObject;

        LiteCollection<T> GetAllItems<T>(string collectionName, int portalId)
            where T : BasicExportObject;

        T GetItem<T>(string collectionName, int id, int portalId) where T : BasicExportObject;

        LiteCollection<T> GetItems<T>(string collectionName, IEnumerable<string> relationIds, int portalId,
            int? top = null) where T : BasicExportObject;

        T UpdateItem<T>(int id, T item, int portalId) where T : BasicExportObject;

        void DeleteItem<T>(int id, string collectionName, int portalId) where T : BasicExportObject;
    }
}