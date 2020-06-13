// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Common
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.ExportImport.Components.Dto;

    public class SummaryList : List<SummaryItem>
    {
        public new void Add(SummaryItem item)
        {
            if (this.Any(x => x.Category == item.Category))
            {
                this.First(x => x.Category == item.Category).TotalItems += item.TotalItems;
                this.First(x => x.Category == item.Category).ProcessedItems += item.ProcessedItems;
            }
            else
            {
                base.Add(item);
            }
        }

        public new void AddRange(IEnumerable<SummaryItem> items)
        {
            var summaryItems = items as IList<SummaryItem> ?? items.ToList();
            foreach (var summaryItem in summaryItems.OrderBy(x => x.Order))
            {
                this.Add(summaryItem);
            }
        }
    }
}
