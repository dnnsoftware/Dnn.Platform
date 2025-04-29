// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco;

using System;

using global::PetaPoco;

[CLSCompliant(false)]
public class FluentColumnMap
{
    /// <summary>Initializes a new instance of the <see cref="FluentColumnMap"/> class.</summary>
    public FluentColumnMap()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FluentColumnMap"/> class.</summary>
    /// <param name="columnInfo"></param>
    public FluentColumnMap(ColumnInfo columnInfo)
        : this(columnInfo, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FluentColumnMap"/> class.</summary>
    /// <param name="columnInfo"></param>
    /// <param name="fromDbConverter"></param>
    public FluentColumnMap(ColumnInfo columnInfo, Func<object, object> fromDbConverter)
        : this(columnInfo, fromDbConverter, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FluentColumnMap"/> class.</summary>
    /// <param name="columnInfo"></param>
    /// <param name="fromDbConverter"></param>
    /// <param name="toDbConverter"></param>
    public FluentColumnMap(ColumnInfo columnInfo, Func<object, object> fromDbConverter, Func<object, object> toDbConverter)
    {
        this.ColumnInfo = columnInfo;
        this.FromDbConverter = fromDbConverter;
        this.ToDbConverter = toDbConverter;
    }

    public ColumnInfo ColumnInfo { get; set; }

    public Func<object, object> FromDbConverter { get; set; }

    public Func<object, object> ToDbConverter { get; set; }
}
