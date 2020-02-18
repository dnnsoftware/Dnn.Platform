// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;

namespace DotNetNuke.Tests.Utilities
{
    public class DnnUnitTest
    {
        public DnnUnitTest()
        {
            var uri = new System.Uri(Assembly.GetExecutingAssembly().CodeBase);
            string path = HttpUtility.UrlDecode(Path.GetFullPath(uri.AbsolutePath));

            WebsiteAppPath = "http://localhost/DNN_Platform";
            var websiteRootPath = path.Substring(0, path.IndexOf("DNN Platform", System.StringComparison.Ordinal));
            WebsitePhysicalAppPath = Path.Combine(websiteRootPath, "Website");
            HighlightDataPath = Path.Combine(websiteRootPath, "DNN Platform//Modules//PreviewProfileManagement//Resources//highlightDevices.xml");
        }

        public string HighlightDataPath { get; set; }

        public string WebsiteAppPath { get; set; }

        public string WebsitePhysicalAppPath { get; set; }
    }
}
