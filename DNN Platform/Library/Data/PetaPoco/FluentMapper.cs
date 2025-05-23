﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Caching;

    using global::PetaPoco;

    [CLSCompliant(false)]
    public class FluentMapper<TModel> : IMapper
    {
        private readonly string tablePrefix;

        /// <summary>Initializes a new instance of the <see cref="FluentMapper{TModel}"/> class.</summary>
        /// <param name="tablePrefix">The prefix for the table.</param>
        public FluentMapper(string tablePrefix)
        {
            this.CacheKey = string.Empty;
            this.CachePriority = CacheItemPriority.Default;
            this.CacheTimeOut = 0;
            this.Mappings = new Dictionary<string, FluentColumnMap>();
            this.Scope = string.Empty;
            this.TableInfo = new TableInfo();
            this.tablePrefix = tablePrefix;
        }

        public string CacheKey { get; set; }

        public CacheItemPriority CachePriority { get; set; }

        public int CacheTimeOut { get; set; }

        public Dictionary<string, FluentColumnMap> Mappings { get; set; }

        public string Scope { get; set; }

        public TableInfo TableInfo { get; set; }

        /// <inheritdoc/>
        public TableInfo GetTableInfo(Type pocoType)
        {
            return this.TableInfo;
        }

        /// <inheritdoc/>
        public ColumnInfo GetColumnInfo(PropertyInfo pocoProperty)
        {
            var fluentMap = default(FluentColumnMap);
            if (this.Mappings.TryGetValue(pocoProperty.Name, out fluentMap))
            {
                return fluentMap.ColumnInfo;
            }

            return null;
        }

        /// <inheritdoc/>
        public Func<object, object> GetFromDbConverter(PropertyInfo targetProperty, Type sourceType)
        {
            // ReSharper disable once RedundantAssignment
            var fluentMap = default(FluentColumnMap);
            if (this.Mappings.TryGetValue(targetProperty.Name, out fluentMap))
            {
                return fluentMap.FromDbConverter;
            }

            return null;
        }

        /// <inheritdoc/>
        public Func<object, object> GetToDbConverter(PropertyInfo sourceProperty)
        {
            // ReSharper disable once RedundantAssignment
            var fluentMap = default(FluentColumnMap);
            if (this.Mappings.TryGetValue(sourceProperty.Name, out fluentMap))
            {
                return fluentMap.ToDbConverter;
            }

            return null;
        }

        public string GetTablePrefix()
        {
            return this.tablePrefix;
        }
    }
}
