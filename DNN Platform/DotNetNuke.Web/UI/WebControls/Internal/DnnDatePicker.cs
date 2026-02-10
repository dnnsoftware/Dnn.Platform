// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    public class DnnDatePicker : TextBox
    {
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="DnnDatePicker"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DnnDatePicker()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnDatePicker"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DnnDatePicker(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        }

        /// <summary>Gets or sets the selected date.</summary>
        public DateTime? SelectedDate
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Text) && DateTime.TryParse(this.Text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var value))
                {
                    return value;
                }

                return null;
            }

            set
            {
                this.Text = value?.ToString(this.Format, CultureInfo.CurrentCulture) ?? string.Empty;
            }
        }

        /// <summary>Gets or sets the minimum date.</summary>
        public DateTime MinDate { get; set; } = new DateTime(1900, 1, 1);

        /// <summary>Gets or sets the maximum date.</summary>
        public DateTime MaxDate { get; set; } = DateTime.MaxValue;

        /// <summary>Gets the .NET format string for the date.</summary>
        protected virtual string Format => "yyyy-MM-dd";

        /// <summary>Gets the moment.js format string.</summary>
        protected virtual string ClientFormat => "YYYY-MM-DD";

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.jQuery);

            this.clientResourceController.RegisterScript("~/Resources/Shared/components/DatePicker/moment.min.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/components/DatePicker/pikaday.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/components/DatePicker/pikaday.jquery.js");

            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/components/DatePicker/pikaday.css");

            this.RegisterClientResources();
        }

        /// <summary>Gets the settings.</summary>
        /// <returns>A dictionary of pikaday settings.</returns>
        protected virtual IDictionary<string, object> GetSettings()
        {
            return new Dictionary<string, object>
            {
                { "minDate", this.MinDate > DateTime.MinValue ? $"$new Date('{HttpUtility.JavaScriptStringEncode(this.MinDate.ToString(this.Format, CultureInfo.InvariantCulture))}')$" : string.Empty },
                { "maxDate", this.MaxDate > DateTime.MinValue ? $"$new Date('{HttpUtility.JavaScriptStringEncode(this.MaxDate.ToString(this.Format, CultureInfo.InvariantCulture))}')$" : string.Empty },
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
