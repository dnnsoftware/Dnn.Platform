// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections.Specialized;
    using System.Web;

    using DotNetNuke.Entities.Urls;

    internal class UrlTestHelper
    {
        public Uri RequestUri { get; set; }

        public UrlAction Result { get; set; }

        public NameValueCollection QueryStringCol { get; set; }

        public string HttpAliasFull { get; set; }

        public HttpResponse Response { get; set; }
    }
}
