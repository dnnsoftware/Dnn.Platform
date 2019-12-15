// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
