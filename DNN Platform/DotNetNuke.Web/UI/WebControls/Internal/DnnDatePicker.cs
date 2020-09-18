// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnDatePicker : TextBox
    {
        public DateTime? SelectedDate
        {
            get
            {
                DateTime value;
                if (!string.IsNullOrEmpty(this.Text) && DateTime.TryParse(this.Text, out value))
                {
                    return value;
                }

                return null;
            }

            set
            {
                this.Text = value?.ToString(this.Format) ?? string.Empty;
            }
        }

        public DateTime MinDate { get; set; } = new DateTime(1900, 1, 1);

        public DateTime MaxDate { get; set; } = DateTime.MaxValue;

        protected virtual string Format => "yyyy-MM-dd";

        protected virtual string ClientFormat => "YYYY-MM-DD";

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            JavaScript.RequestRegistration(CommonJs.jQuery);

            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/DatePicker/moment.min.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/DatePicker/pikaday.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/DatePicker/pikaday.jquery.js");

            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/components/DatePicker/pikaday.css");

            this.RegisterClientResources();
        }

        protected virtual IDictionary<string, object> GetSettings()
        {
            return new Dictionary<string, object>
            {
                { "minDate", this.MinDate > DateTime.MinValue ? $"$new Date('{this.MinDate.ToString(this.Format, CultureInfo.InvariantCulture)}')$" : string.Empty },
                { "maxDate", this.MaxDate > DateTime.MinValue ? $"$new Date('{this.MaxDate.ToString(this.Format, CultureInfo.InvariantCulture)}')$" : string.Empty },
                { "format", this.ClientFormat },
            };
        }

        private void RegisterClientResources()
        {
            var settings = Json.Serialize(this.GetSettings()).Replace("\"$", string.Empty).Replace("$\"", string.Empty);
            var script = $"$('#{this.ClientID}').pikaday({settings});";

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "DnnDatePicker" + this.ClientID, script, true);
        }
    }
}
