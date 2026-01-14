// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    internal static class AuthExtensions
    {
        public static string ToAuthorizationString(this IList<QueryParameter> parameters)
        {
            var sb = new StringBuilder();
            sb.Append("OAuth ");

            for (var i = 0; i < parameters.Count; i++)
            {
                const string format = "{0}=\"{1}\"";

                var p = parameters[i];
                sb.AppendFormat(CultureInfo.InvariantCulture, format, OAuthClientBase.UrlEncode(p.Name), OAuthClientBase.UrlEncode(p.Value));

                if (i < parameters.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        public static string ToNormalizedString(this IList<QueryParameter> parameters)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append('&');
                }
            }

            return sb.ToString();
        }
    }
}
