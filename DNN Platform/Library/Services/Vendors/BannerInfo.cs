#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
#region Usings

using System;

using DotNetNuke.Services.FileSystem;

#endregion

namespace DotNetNuke.Services.Vendors
{
    [Serializable]
    public class BannerInfo
    {
        public int BannerId { get; set; }
        public int VendorId { get; set; }
        public string ImageFile { get; set; }
        public string BannerName { get; set; }
        public string URL { get; set; }
        public int Impressions { get; set; }
        // ReSharper disable InconsistentNaming
        // Existing public API
        public double CPM { get; set; }
        // ReSharper restore InconsistentNaming
        public int Views { get; set; }
        public int ClickThroughs { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public int BannerTypeId { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public int Criteria { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ImageFileRaw { get; set; }
        public string ImageFileUrl { get
        {
            if(ImageFileRaw.StartsWith("FileID="))
            {
                int fileId = int.Parse(ImageFileRaw.Substring("FileID=".Length));
                var file = FileManager.Instance.GetFile(fileId);                
                return file != null ? FileManager.Instance.GetUrl(file) : "";
            }

            return ImageFileRaw;
        } }
    }
}