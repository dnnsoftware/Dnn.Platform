// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Prompt
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The repository handles retrieving of commands from the entire DNN installation.
    /// </summary>
    [Obsolete("Deprecated in v9.9.1. Use IDnnCommandRepository instead. Will be removed in v11.", false)]
    public interface ICommandRepository : IDnnCommandRepository
    {
    }
}
