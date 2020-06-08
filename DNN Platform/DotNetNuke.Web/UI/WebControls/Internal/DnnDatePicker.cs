// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnDatePicker : TextBox
    {
        protected virtual string Format => "yyyy-MM-dd";
        protected virtual string ClientFormat => "YYYY-MM-DD";

        public DateTime? SelectedDate {
            get
            {
                DateTime value;
                if (!string.IsNullOrEmpty(Text) && DateTime.TryParse(Text, out value))
                {
                    return value;
                }

                return null;
            }
            set
            {
                Text = value?.ToString(Format) ?? string.Empty;
            }
        }

        public DateTime MinDate { get; set; } = new DateTime(1900, 1, 1);

        public DateTime MaxDate { get; set; } = DateTime.MaxValue;


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            JavaScript.RequestRegistration(CommonJs.jQuery);

            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/components/DatePicker/moment.min.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/components/DatePicker/pikaday.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/components/DatePicker/pikaday.jquery.js");

            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/components/DatePicker/pikaday.css");

            RegisterClientResources();
        }

        protected virtual IDictionary<string, object> GetSettings()
        {
            return new Dictionary<string, object>
            {
                {"minDate", MinDate > DateTime.MinValue ? $"$new Date('{MinDate.ToString(Format, CultureInfo.InvariantCulture)}')$" : ""},
                {"maxDate", MaxDate > DateTime.MinValue ? $"$new Date('{MaxDate.ToString(Format, CultureInfo.InvariantCulture)}')$" : ""},
                {"format", ClientFormat }
            };
        } 

        private void RegisterClientResources()
        {
            var settings = Json.Serialize(GetSettings()).Replace("\"$", "").Replace("$\"", "");
            var script = $"$('#{ClientID}').pikaday({settings});";

            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "DnnDatePicker" + ClientID, script, true);
        }
    }
}
