// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AzureConnector.Components;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an issue in the Azure connector.</summary>
[Serializable]
public class AzureConnectorException : Exception
{
    /// <inheritdoc />
    public AzureConnectorException()
    {
    }

    /// <inheritdoc />
    public AzureConnectorException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public AzureConnectorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected AzureConnectorException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
