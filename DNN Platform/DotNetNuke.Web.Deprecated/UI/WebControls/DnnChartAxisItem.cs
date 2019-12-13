// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Drawing;

using Telerik.Charting;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnChartAxisItem : ChartAxisItem
    {
        public DnnChartAxisItem()
        {
        }

        public DnnChartAxisItem(string labelText) : base(labelText)
        {
        }

        public DnnChartAxisItem(string labelText, Color color) : base(labelText, color)
        {
        }

        public DnnChartAxisItem(string labelText, Color color, bool visible) : base(labelText, color, visible)
        {
        }

        public DnnChartAxisItem(string labelText, Color color, bool visible, IContainer container) : base(labelText, color, visible, container)
        {
        }

        public DnnChartAxisItem(IContainer container) : base(container)
        {
        }
    }
}
