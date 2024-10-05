// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities;

using System;

using DotNetNuke.Abstractions.Skins;

/// <summary>
/// Skin utilities.
/// </summary>
public class SkinUtils
{
    /// <summary>Gets the database name of a skin package type.</summary>
    /// <param name="type">The type of the skin package.</param>
    /// <returns>The database name of the skin package type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is not a valid skin package type.</exception>
    public static string ToDatabaseName(SkinPackageType type)
    {
        return type switch
        {
            SkinPackageType.Skin => "Skin",
            SkinPackageType.Container => "Container",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid skin package type."),
        };
    }

    /// <summary>Gets the skin package type from a database name.</summary>
    /// <param name="databaseName">The database name of the skin package type.</param>
    /// <returns>The skin package type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="databaseName"/> is not a valid skin package type database name.</exception>
    public static SkinPackageType FromDatabaseName(string databaseName)
    {
        if (databaseName == null)
        {
            throw new ArgumentNullException(nameof(databaseName));
        }

        if (string.Equals(databaseName, "Skin", StringComparison.OrdinalIgnoreCase))
        {
            return SkinPackageType.Skin;
        }

        if (string.Equals(databaseName, "Container", StringComparison.OrdinalIgnoreCase))
        {
            return SkinPackageType.Container;
        }

        throw new ArgumentOutOfRangeException(nameof(databaseName), databaseName, "Invalid skin package type database name.");
    }
}
