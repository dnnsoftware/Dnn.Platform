// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>IAsyncSettingsControl provides a common Interface for Module Settings Controls that need to execute async work.</summary>
    public interface IAsyncSettingsControl : ISettingsControl
    {
        /// <summary>Loads the module settings asynchronously.</summary>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task LoadSettingsAsync(CancellationToken cancellationToken);

        /// <summary>Updates the module settings asynchronously.</summary>
        /// <param name="cancellationToken">cancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateSettingsAsync(CancellationToken cancellationToken);
    }
}
