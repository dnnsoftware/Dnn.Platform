// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Drawing;

    using Telerik.Charting;
    using Telerik.Charting.Styles;

    public class DnnChartSeriesItem : ChartSeriesItem
    {
        public DnnChartSeriesItem()
        {
        }

        public DnnChartSeriesItem(bool isEmpty)
            : base(isEmpty)
        {
        }

        public DnnChartSeriesItem(double value)
            : base(value)
        {
        }

        public DnnChartSeriesItem(double x, double y)
            : base(x, y)
        {
        }

        public DnnChartSeriesItem(double x, double y, double x2, double y2)
            : base(x, y, x2, y2)
        {
        }

        public DnnChartSeriesItem(double x, double y, double x2, double y2, double y3, double y4)
            : base(x, y, x2, y2, y3, y4)
        {
        }

        public DnnChartSeriesItem(double x, double y, StyleSeriesItem style)
            : base(x, y, style)
        {
        }

        public DnnChartSeriesItem(double value, string labelText)
            : base(value, labelText)
        {
        }

        public DnnChartSeriesItem(double value, string label, Color color)
            : base(value, label, color)
        {
        }

        public DnnChartSeriesItem(double value, string label, Color color, bool exploded)
            : base(value, label, color, exploded)
        {
        }

        public DnnChartSeriesItem(ChartSeries parent)
            : base(parent)
        {
        }
    }
}
