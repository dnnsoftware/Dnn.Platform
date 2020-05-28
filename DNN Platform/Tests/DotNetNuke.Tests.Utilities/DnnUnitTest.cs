// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
