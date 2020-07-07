// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using Telerik.Charting;
    using Telerik.Charting.Styles;

    public class DnnChartSeries : ChartSeries
    {
        public DnnChartSeries()
        {
        }

        public DnnChartSeries(string name)
            : base(name)
        {
        }

        public DnnChartSeries(string name, ChartSeriesType type)
            : base(name, type)
        {
        }

        public DnnChartSeries(string name, ChartSeriesType type, ChartSeriesCollection parent)
            : base(name, type, parent)
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

        public DnnChartSeries(ChartSeriesCollection parent)
            : base(parent)
        {
        }
    }
}
