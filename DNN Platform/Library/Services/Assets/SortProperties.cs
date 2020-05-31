// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Assets
{
    public class SortProperties
    {
        public string Column { get; set; }

        public bool Ascending { get; set; }

        public static SortProperties Parse(string sortExpression)
        {
            var sortProperties = new SortProperties { Column = "ItemName", Ascending = true };

            if (!string.IsNullOrEmpty(sortExpression))
            {
                var se = sortExpression.Split(' ');
                if (se.Length == 2)
                {
                    sortProperties.Column = se[0];
                    sortProperties.Ascending = se[1] == "ASC";
                }
            }

            return sortProperties;
        }
    }
}
