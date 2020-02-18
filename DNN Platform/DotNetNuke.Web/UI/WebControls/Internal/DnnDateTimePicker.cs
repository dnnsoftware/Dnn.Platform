// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
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
