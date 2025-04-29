// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections;

/// <summary>This interface is used to make a class indexable.</summary>
/// <example>
/// <code>
/// var phone = userInfo.Profile["Telephone"];
/// </code>
/// </example>
internal interface IIndexable
{
    /// <summary>Returns an object that depends on the string provided for the index.</summary>
    /// <param name="name">The string that represents the index value.</param>
    /// <returns>An object.</returns>
    object this[string name] { get; set; }
}
