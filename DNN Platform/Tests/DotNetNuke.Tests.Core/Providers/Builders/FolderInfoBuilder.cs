#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
