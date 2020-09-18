// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Builders
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities;
    using Moq;

    internal class FolderInfoBuilder
    {
        private int portalId;
        private int folderId;
        private string folderPath;
        private string physicalPath;
        private int folderMappingID;

        internal FolderInfoBuilder()
        {
            this.portalId = Constants.CONTENT_ValidPortalId;
            this.folderPath = Constants.FOLDER_ValidFolderRelativePath;
            this.physicalPath = Constants.FOLDER_ValidFolderPath;
            this.folderMappingID = Constants.FOLDER_ValidFolderMappingID;
            this.folderId = Constants.FOLDER_ValidFolderId;
            this.physicalPath = string.Empty;
        }

        internal FolderInfoBuilder WithPhysicalPath(string phisicalPath)
        {
            this.physicalPath = phisicalPath;
            return this;
        }

        internal FolderInfoBuilder WithFolderId(int folderId)
        {
            this.folderId = folderId;
            return this;
        }

        internal IFolderInfo Build()
        {
            var mock = new Mock<IFolderInfo>();
            mock.Setup(f => f.FolderID).Returns(this.folderId);
            mock.Setup(f => f.PortalID).Returns(this.portalId);
            mock.Setup(f => f.FolderPath).Returns(this.folderPath);
            mock.Setup(f => f.PhysicalPath).Returns(this.physicalPath);
            mock.Setup(f => f.FolderMappingID).Returns(this.folderMappingID);

            return mock.Object;
        }
    }
}
