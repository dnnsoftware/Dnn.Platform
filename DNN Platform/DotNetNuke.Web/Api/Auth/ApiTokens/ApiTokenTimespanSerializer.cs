// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using DotNetNuke.Entities.Modules.Settings;

    public class ApiTokenTimespanSerializer : ISettingsSerializer<ApiTokenTimespan>
    {
        public ApiTokenTimespan Deserialize(string value)
        {
            return (ApiTokenTimespan)int.Parse(value);
        }

        public string Serialize(ApiTokenTimespan value)
        {
            return ((int)value).ToString();
        }
    }
}
