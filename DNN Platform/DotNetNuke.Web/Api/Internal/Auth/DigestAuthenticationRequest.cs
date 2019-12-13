﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace DotNetNuke.Web.Api.Internal.Auth
{
    internal class DigestAuthenticationRequest
    {
        private static readonly Regex AuthHeaderRegex = new Regex("\\s?(?'name'\\w+)=(\"(?'value'[^\"]+)\"|(?'value'[^,]+))", RegexOptions.Compiled);

        public DigestAuthenticationRequest(string authorizationHeader, string httpMethod)
        {
            //Authorization: Digest
            //username="Mufasa",
            //realm="testrealm@host.com",
            //nonce="dcd98b7102dd2f0e8b11d0f600bfb0c093",
            //uri="/dir/index.html",
            //qop=auth,
            //nc=00000001,
            //cnonce="0a4f113b",
            //response="6629fae49393a05397450978507c4ef1",
            //opaque="5ccc069c403ebaf9f0171e9517f40e41"
            try
            {
                RequestParams = new NameValueCollection();
                foreach (Match m in AuthHeaderRegex.Matches(authorizationHeader))
                {
                    RequestParams.Add(m.Groups["name"].Value, m.Groups["value"].Value);
                }
                HttpMethod = httpMethod;
                RawUsername = RequestParams["username"].Replace("\\\\", "\\");
                CleanUsername = RawUsername;
                if (CleanUsername.LastIndexOf("\\", System.StringComparison.Ordinal) > 0)
                {
                    CleanUsername = CleanUsername.Substring(CleanUsername.LastIndexOf("\\", System.StringComparison.Ordinal) + 2 - 1);
                }
            }
            catch (Exception)
            {
                
                //suppress any issue e.g. another 401 from a different auth method
            }
            
        }

        public NameValueCollection RequestParams { get; set; }

        public string CleanUsername { get; private set; }

        public string RawUsername { get; private set; }

        public string HttpMethod { get; set; }
    }
}
