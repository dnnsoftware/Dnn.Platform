// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using Telerik.Web.UI;

    public class DnnTagCloud : RadTagCloud
    {
        protected void OnItemDataBound(DnnTagCloudItem item)
        {
            base.OnItemDataBound(item);
        }
    }
}
