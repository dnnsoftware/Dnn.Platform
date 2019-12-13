// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.Authentication.OAuth
{
    /// <summary>
    /// Provides an internal structure to sort the query parameter
    /// </summary>
    public class QueryParameter
    {
        public QueryParameter(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get;private set; }

        public string Value { get;private set; }
    }
}
