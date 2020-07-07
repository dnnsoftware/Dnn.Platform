// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AzureConnector.Components
{
    internal class Constants
    {
        public const string FolderProviderType = "AzureFolderProvider";
        public const string AzureAccountName = "AccountName";
        public const string AzureAccountKey = "AccountKey";
        public const string AzureContainerName = "Container";
        public const string DirectLink = "DirectLink";
        public const string UseHttps = "UseHttps";
        public const string SyncBatchSize = "SyncBatchSize";
        public const int DefaultSyncBatchSize = 2048;

        public const string LocalResourceFile =
            "~/DesktopModules/Connectors/Azure/App_LocalResources/SharedResources.resx";
    }
}
