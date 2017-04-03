#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace Dnn.ExportImport.Components.Dto
{
    [JsonObject]
    public class ImportPackageInfo : IDateTimeConverter
    {
        /// <summary>
        /// Package Id. Used to identify the package and path.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// Name of the package.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Package file name. It is just fake name for UI representation
        /// </summary>
        public string FileName => PackageId;

        /// <summary>
        /// DateTime when the package was exported.
        /// </summary>
        public DateTime ExporTime { get; set; }

        /// <summary>
        /// Formatted export file size
        /// </summary>
        public string ExportSize { get; set; }

        /// <summary>
        /// The portal from which the exported package was created
        /// </summary>
        public string PortalName { get; set; }

        /// <summary>
        /// Package description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Path to the thumbnail image for the package.
        /// </summary>
        public string Thumb => PackageId + ".jpg";

        public void ConvertToLocal(UserInfo userInfo)
        {
            if (userInfo == null) return;
            ExporTime = userInfo.LocalTime(ExporTime);
        }
    }
}
