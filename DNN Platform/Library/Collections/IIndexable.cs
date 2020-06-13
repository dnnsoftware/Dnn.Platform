// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections
{
    using System;

    /// <summary>
    /// This interface used to make a class can have index declaration.
    /// </summary>
    internal interface IIndexable
    {
        object this[string name] { get; set; }
    }
}
