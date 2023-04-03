// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System.Collections.Generic;

    /// <summary>An interface that represent an abstract process step that contains a set of nested steps.</summary>
    internal interface IStepArray : IStep
    {
        /// <summary>Gets an <see cref="IEnumerable{T}"/> of <see cref="IStep"/> instances contained in this array step.</summary>
        IEnumerable<IStep> Steps { get; }
    }
}
