#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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