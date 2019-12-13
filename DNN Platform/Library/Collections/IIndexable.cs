// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
namespace DotNetNuke.Collections
{
    /// <summary>
    /// This interface used to make a class can have index declaration.
    /// </summary>
    internal interface IIndexable
    {
        object this[string name] { get; set; }
    }
}
