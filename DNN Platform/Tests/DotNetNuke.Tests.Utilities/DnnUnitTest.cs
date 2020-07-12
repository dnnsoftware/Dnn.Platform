// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Web;

    public class DnnUnitTest
    {
        public DnnUnitTest()
        {
            var uri = new System.Uri(Assembly.GetExecutingAssembly().CodeBase);
            string path = HttpUtility.UrlDecode(Path.GetFullPath(uri.AbsolutePath));

            this.WebsiteAppPath = "http://localhost/DNN_Platform";
            var websiteRootPath = path.Substring(0, path.IndexOf("DNN Platform", System.StringComparison.Ordinal));
            this.WebsitePhysicalAppPath = Path.Combine(websiteRootPath, "Website");
            this.HighlightDataPath = Path.Combine(websiteRootPath, "DNN Platform//Modules//PreviewProfileManagement//Resources//highlightDevices.xml");
        }

        public string HighlightDataPath { get; set; }

        public string WebsiteAppPath { get; set; }

        public string WebsitePhysicalAppPath { get; set; }
    }
}
