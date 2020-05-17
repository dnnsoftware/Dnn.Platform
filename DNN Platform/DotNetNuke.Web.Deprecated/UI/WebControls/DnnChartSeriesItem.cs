﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Drawing;

using Telerik.Charting;
using Telerik.Charting.Styles;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnChartSeriesItem : ChartSeriesItem
    {
        public DnnChartSeriesItem()
        {
        }

        public DnnChartSeriesItem(bool isEmpty) : base(isEmpty)
        {
        }

        public DnnChartSeriesItem(double value) : base(value)
        {
        }

        public DnnChartSeriesItem(double x, double y) : base(x, y)
        {
        }

        public DnnChartSeriesItem(double x, double y, double x2, double y2) : base(x, y, x2, y2)
        {
        }

        public DnnChartSeriesItem(double x, double y, double x2, double y2, double y3, double y4) : base(x, y, x2, y2, y3, y4)
        {
        }

        public DnnChartSeriesItem(double x, double y, StyleSeriesItem style) : base(x, y, style)
        {
        }

        public DnnChartSeriesItem(double value, string labelText) : base(value, labelText)
        {
        }

        public DnnChartSeriesItem(double value, string label, Color color) : base(value, label, color)
        {
        }

        public DnnChartSeriesItem(double value, string label, Color color, bool exploded) : base(value, label, color, exploded)
        {
        }

        public DnnChartSeriesItem(ChartSeries parent) : base(parent)
        {
        }
    }
}
