// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search;

using System;
using System.Runtime.Serialization;

#pragma warning restore 0618
/// <summary>An exception thrown when a business controller class cannot be created.</summary>
[Serializable]
public class BusinessControllerClassException : Exception
{
    /// <inheritdoc />
    public BusinessControllerClassException()
    {
    }

    /// <inheritdoc />
    public BusinessControllerClassException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public BusinessControllerClassException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected BusinessControllerClassException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
