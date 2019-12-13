// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
// 
// Influenced by http://sameproblemmorecode.blogspot.nl/2013/07/petapoco-as-its-meant-to-be-with.html
// https://github.com/luuksommers/PetaPoco

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Caching;
using PetaPoco;

namespace DotNetNuke.Data.PetaPoco
{
    [CLSCompliant(false)]
    public class FluentMapper<TModel> : IMapper
    {
        private readonly string _tablePrefix;

        public FluentMapper(string tablePrefix)
        {
            CacheKey = String.Empty;
            CachePriority = CacheItemPriority.Default;
            CacheTimeOut = 0;
            Mappings = new Dictionary<string, FluentColumnMap>();
            Scope = String.Empty;
            TableInfo = new TableInfo();
            _tablePrefix = tablePrefix;
        }

        public string CacheKey { get; set; }

        public CacheItemPriority CachePriority { get; set; }

        public int CacheTimeOut { get; set; }

        public Dictionary<string, FluentColumnMap> Mappings { get; set; }

        public string Scope { get; set; }

        public TableInfo TableInfo { get; set; }

        public TableInfo GetTableInfo(Type pocoType)
        {
            return TableInfo;
        }

        public ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
        {
            var fluentMap = default(FluentColumnMap);
            if (Mappings.TryGetValue(pocoProperty.Name, out fluentMap))
                return fluentMap.ColumnInfo;
            return null;
        }

        public Func<object, object> GetFromDbConverter(PropertyInfo targetProperty, Type sourceType)
        {
            // ReSharper disable once RedundantAssignment
            var fluentMap = default(FluentColumnMap);
            if (Mappings.TryGetValue(targetProperty.Name, out fluentMap))
                return fluentMap.FromDbConverter;
            return null;
        }

        public Func<object, object> GetToDbConverter(PropertyInfo sourceProperty)
        {
            // ReSharper disable once RedundantAssignment
            var fluentMap = default(FluentColumnMap);
            if (Mappings.TryGetValue(sourceProperty.Name, out fluentMap))
                return fluentMap.ToDbConverter;
            return null;
        }

        public string GetTablePrefix()
        {
            return _tablePrefix;
        }
    }
}
