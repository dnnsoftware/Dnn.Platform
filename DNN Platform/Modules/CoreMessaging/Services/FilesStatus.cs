// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    public class FilesStatus
    {
        // ReSharper restore InconsistentNaming

        public FilesStatus()
        {
        } // ReSharper disable InconsistentNaming
        public bool success { get; set; }

        public string name { get; set; }

        public string extension { get; set; }

        public string type { get; set; }

        public int size { get; set; }

        public string progress { get; set; }

        public string url { get; set; }

        public string thumbnail_url { get; set; }

        public string message { get; set; }

        public int id { get; set; }
    }
}
