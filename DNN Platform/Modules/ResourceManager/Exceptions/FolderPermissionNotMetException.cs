// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Exceptions;

using System;

/// <summary>Exception thrown when a folder permission is not met.</summary>
public class FolderPermissionNotMetException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="FolderPermissionNotMetException"/> class.</summary>
    /// <param name="message">The exception message.</param>
    public FolderPermissionNotMetException(string message)
        : base(message)
    {
    }
}
