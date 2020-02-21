// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Globalization;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
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

            return ProcessToken(deserializedObject, accessingUser, accessLevel);
        }

        protected abstract string ProcessToken(TModel model, UserInfo accessingUser, Scope accessLevel);
    }
}
