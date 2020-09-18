// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using Newtonsoft.Json;

    public abstract class JsonPropertyAccess<TModel> : IPropertyAccess
    {
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            var json = propertyName.Trim();
            if (!(json.StartsWith("{") && json.EndsWith("}")))
            {
                throw new ArgumentException("The token argument is not property formatted JSON");
            }

            var deserializedObject = JsonConvert.DeserializeObject<TModel>(json);

            return this.ProcessToken(deserializedObject, accessingUser, accessLevel);
        }

        protected abstract string ProcessToken(TModel model, UserInfo accessingUser, Scope accessLevel);
    }
}
