// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal.Auth
{
    using System;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;

    internal class DigestAuthenticationRequest
    {
        private static readonly Regex AuthHeaderRegex = new Regex("\\s?(?'name'\\w+)=(\"(?'value'[^\"]+)\"|(?'value'[^,]+))", RegexOptions.Compiled);

        public DigestAuthenticationRequest(string authorizationHeader, string httpMethod)
        {
            // Authorization: Digest
            // username="Mufasa",
            // realm="testrealm@host.com",
            // nonce="dcd98b7102dd2f0e8b11d0f600bfb0c093",
            // uri="/dir/index.html",
            // qop=auth,
            // nc=00000001,
            // cnonce="0a4f113b",
            // response="6629fae49393a05397450978507c4ef1",
            // opaque="5ccc069c403ebaf9f0171e9517f40e41"
            try
            {
                this.RequestParams = new NameValueCollection();
                foreach (Match m in AuthHeaderRegex.Matches(authorizationHeader))
                {
                    this.RequestParams.Add(m.Groups["name"].Value, m.Groups["value"].Value);
                }

                this.HttpMethod = httpMethod;
                this.RawUsername = this.RequestParams["username"].Replace("\\\\", "\\");
                this.CleanUsername = this.RawUsername;
                if (this.CleanUsername.LastIndexOf("\\", System.StringComparison.Ordinal) > 0)
                {
                    this.CleanUsername = this.CleanUsername.Substring(this.CleanUsername.LastIndexOf("\\", System.StringComparison.Ordinal) + 2 - 1);
                }
            }
            catch (Exception)
            {
                // suppress any issue e.g. another 401 from a different auth method
            }
        }

        public NameValueCollection RequestParams { get; set; }

        public string CleanUsername { get; private set; }

        public string RawUsername { get; private set; }

        public string HttpMethod { get; set; }
    }
}
