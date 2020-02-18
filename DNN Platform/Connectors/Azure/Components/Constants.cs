// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
﻿namespace Dnn.AzureConnector.Components
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
