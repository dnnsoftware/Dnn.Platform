// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.AzureConnector.Components
{
    /// <summary>Azure connector constants.</summary>
    internal static class Constants
    {
        /// <summary>The folder provider type.</summary>
        public const string FolderProviderType = "AzureFolderProvider";

        /// <summary>The Azure account name setting name.</summary>
        public const string AzureAccountName = "AccountName";

        /// <summary>The Azure account key setting name.</summary>
        public const string AzureAccountKey = "AccountKey";

        /// <summary>The Azure container name setting name.</summary>
        public const string AzureContainerName = "Container";

        /// <summary>The direct link setting name.</summary>
        public const string DirectLink = "DirectLink";

        /// <summary>The sync batch size setting name.</summary>
        public const string UseHttps = "UseHttps";

        /// <summary>The sync batch size setting name.</summary>
        public const string SyncBatchSize = "SyncBatchSize";

        /// <summary>The default sync batch size.</summary>
        public const int DefaultSyncBatchSize = 2048;

        /// <summary>The local resource file.</summary>
        public const string LocalResourceFile =
            "~/DesktopModules/Connectors/Azure/App_LocalResources/SharedResources.resx";
    }
}
