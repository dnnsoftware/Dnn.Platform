// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Common;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when issues are encountered importing.</summary>
[Serializable]
public class ImportException : Exception
{
    /// <inheritdoc />
    public ImportException()
    {
    }

    /// <inheritdoc />
    public ImportException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public ImportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected ImportException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
