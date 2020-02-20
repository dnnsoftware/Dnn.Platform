// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Tests.Utilities;
using Moq;

namespace DotNetNuke.Tests.Core.Providers.Builders
{
    internal class FolderInfoBuilder
    {
        private int portalId;
        private int folderId;
        private string folderPath;
        private string physicalPath;
        private int folderMappingID;
        
        internal FolderInfoBuilder()
        {
            portalId = Constants.CONTENT_ValidPortalId;
            folderPath = Constants.FOLDER_ValidFolderRelativePath;
            physicalPath = Constants.FOLDER_ValidFolderPath;
            folderMappingID = Constants.FOLDER_ValidFolderMappingID;
            folderId = Constants.FOLDER_ValidFolderId;
            physicalPath = "";
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
            mock.Setup(f => f.FolderID).Returns(folderId);
            mock.Setup(f => f.PortalID).Returns(portalId);
            mock.Setup(f => f.FolderPath).Returns(folderPath);
            mock.Setup(f => f.PhysicalPath).Returns(physicalPath);
            mock.Setup(f => f.FolderMappingID).Returns(folderMappingID);
            
            return mock.Object;
        }
    }
}
