// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Data.PetaPoco;

using System.Collections.Generic;

using DotNetNuke.Collections;
using DotNetNuke.Common;
using global::PetaPoco;

public class PetaPocoRepository<T> : RepositoryBase<T>
    where T : class
{
    private readonly Database database;
    private readonly IMapper mapper;

    /// <summary>Initializes a new instance of the <see cref="PetaPocoRepository{T}"/> class.</summary>
    /// <param name="database"></param>
    /// <param name="mapper"></param>
    public PetaPocoRepository(Database database, IMapper mapper)
    {
        Requires.NotNull("database", database);

        this.database = database;
        this.mapper = mapper;

        PetaPocoMapper.SetMapper<T>(mapper);
    }

    /// <inheritdoc/>
    public override void Delete(string sqlCondition, params object[] args)
    {
        this.database.Delete<T>(DataUtil.ReplaceTokens(sqlCondition), args);
    }

    /// <inheritdoc/>
    public override IEnumerable<T> Find(string sqlCondition, params object[] args)
    {
        return this.database.Fetch<T>(DataUtil.ReplaceTokens(sqlCondition), args);
    }

    /// <inheritdoc/>
    public override IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
    {
        // Make sure that the sql Condition contains an ORDER BY Clause
        if (!sqlCondition.ToUpperInvariant().Contains("ORDER BY"))
        {
            sqlCondition = string.Format("{0} ORDER BY {1}", sqlCondition, this.mapper.GetTableInfo(typeof(T)).PrimaryKey);
        }

        Page<T> petaPocoPage = this.database.Page<T>(pageIndex + 1, pageSize, DataUtil.ReplaceTokens(sqlCondition), args);

        return new PagedList<T>(petaPocoPage.Items, (int)petaPocoPage.TotalItems, pageIndex, pageSize);
    }

    /// <inheritdoc/>
    public override void Update(string sqlCondition, params object[] args)
    {
        this.database.Update<T>(DataUtil.ReplaceTokens(sqlCondition), args);
    }

    /// <inheritdoc/>
    protected override void DeleteInternal(T item)
    {
        this.database.Delete(item);
    }

    /// <inheritdoc/>
    protected override IEnumerable<T> GetInternal()
    {
        return this.database.Fetch<T>(string.Empty);
    }

    /// <inheritdoc/>
    protected override IPagedList<T> GetPageInternal(int pageIndex, int pageSize)
    {
        return this.Find(pageIndex, pageSize, string.Empty);
    }

    /// <inheritdoc/>
    protected override IEnumerable<T> GetByScopeInternal(object propertyValue)
    {
        return this.database.Fetch<T>(this.GetScopeSql(), propertyValue);
    }

    /// <inheritdoc/>
    protected override IPagedList<T> GetPageByScopeInternal(object propertyValue, int pageIndex, int pageSize)
    {
        return this.Find(pageIndex, pageSize, this.GetScopeSql(), propertyValue);
    }

    /// <inheritdoc/>
    protected override T GetByIdInternal(object id)
    {
        return this.database.SingleOrDefault<T>(id);
    }

    /// <inheritdoc/>
    protected override void InsertInternal(T item)
    {
        this.database.Insert(item);
    }

    /// <inheritdoc/>
    protected override void UpdateInternal(T item)
    {
        this.database.Update(item);
    }

    private string GetScopeSql()
    {
        return string.Format("WHERE {0} = @0", DataUtil.GetColumnName(typeof(T), this.Scope));
    }
}
