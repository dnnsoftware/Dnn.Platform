#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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
