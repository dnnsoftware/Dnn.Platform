// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls;

using System;
using System.Runtime.Serialization;

/// <summary>An exception throw when there's an error calling <see cref="CountryLookup.SeekCountry"/>.</summary>
[Serializable]
public class SeekCountryException : Exception
{
    /// <inheritdoc />
    public SeekCountryException()
    {
    }

    /// <inheritdoc />
    public SeekCountryException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public SeekCountryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected SeekCountryException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
