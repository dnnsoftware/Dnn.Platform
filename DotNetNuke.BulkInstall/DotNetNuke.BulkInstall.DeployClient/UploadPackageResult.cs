// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient
{
    public sealed record UploadPackageResult(Task UploadTask, string PackageName, Stream Stream) : IAsyncDisposable
    {
        /// <summary>The upload progress event handler.</summary>
        public event EventHandler<double>? OnProgress;

        /// <summary>Invoke the on progress event.</summary>
        /// <param name="progress">The progress.</param>
        public void TriggerProgress(double progress) => this.OnProgress?.Invoke(this, progress);

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await this.Stream.DisposeAsync();
        }
    }
}
