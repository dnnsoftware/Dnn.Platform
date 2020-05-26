// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Telerik.Charting;
using Telerik.Charting.Styles;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnChartSeries : ChartSeries
    {
        public DnnChartSeries()
        {
        }

        public DnnChartSeries(string name) : base(name)
        {
        }

        public DnnChartSeries(string name, ChartSeriesType type) : base(name, type)
        {
        }

        public DnnChartSeries(string name, ChartSeriesType type, ChartSeriesCollection parent) : base(name, type, parent)
        {
        }

        public DnnChartSeries(string seriesName, ChartSeriesType chartSeriesType, ChartSeriesCollection parent, ChartYAxisType yAxisType, StyleSeries style)
            : base(seriesName, chartSeriesType, parent, yAxisType, style)
        {
        }

        public DnnChartSeries(string seriesName, ChartSeriesType chartSeriesType, ChartSeriesCollection parent, ChartYAxisType yAxisType, StyleSeries style, string dataYColumn, string dataXColumn,
                              string dataYColumn2, string dataXColumn2, string dataYColumn3, string dataYColumn4, string dataLabelsColumn)
            : base(seriesName, chartSeriesType, parent, yAxisType, style, dataYColumn, dataXColumn, dataYColumn2, dataXColumn2, dataYColumn3, dataYColumn4, dataLabelsColumn)
        {
        }

        public DnnChartSeries(ChartSeriesCollection parent) : base(parent)
        {
        }
    }
}
