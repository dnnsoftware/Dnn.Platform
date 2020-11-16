// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;

    using System.Drawing;

    using Telerik.Charting;

    [Obsolete("Telerik support will be removed in DNN Platform 10.0.0.  You will need to find an alternative solution")]
    public class DnnChartAxisItem : ChartAxisItem
    {
        public DnnChartAxisItem()
        {
        }

        public DnnChartAxisItem(string labelText)
            : base(labelText)
        {
        }

        public DnnChartAxisItem(string labelText, Color color)
            : base(labelText, color)
        {
        }

        public DnnChartAxisItem(string labelText, Color color, bool visible)
            : base(labelText, color, visible)
        {
        }

        public DnnChartAxisItem(string labelText, Color color, bool visible, IContainer container)
            : base(labelText, color, visible, container)
        {
        }

        public DnnChartAxisItem(IContainer container)
            : base(container)
        {
        }
    }
}
