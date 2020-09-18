// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System.Collections.Generic;

    /// <remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnDateTimePicker : DnnDatePicker
    {
        protected override string Format => "yyyy-MM-dd HH:mm:ss";

        protected override string ClientFormat => "YYYY-MM-DD HH:mm:ss";

        protected override IDictionary<string, object> GetSettings()
        {
            var settings = base.GetSettings();

            settings.Add("showTime", true);
            settings.Add("use24hour", true);
            settings.Add("autoClose", true);

            return settings;
        }
    }
}
