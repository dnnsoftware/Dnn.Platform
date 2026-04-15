// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.3.0. Please use TextBox with TextMode=TextBoxMode.Date. Scheduled removal in v12.0.0.")]
    public class DnnDatePicker : TextBox
    {
        /// <summary>Initializes a new instance of the <see cref="DnnDatePicker"/> class.</summary>
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

            var settings = this.GetSettings();
            foreach (var setting in settings)
            {
                this.Attributes[setting.Key] = setting.Value?.ToString();
            }
        }

        /// <summary>Gets the settings.</summary>
        /// <returns>A dictionary of attribute values.</returns>
        protected virtual IDictionary<string, object> GetSettings()
        {
            return new Dictionary<string, object>
            {
                { "min", this.MinDate > DateTime.MinValue ? this.MinDate.ToString(this.Format, CultureInfo.InvariantCulture) : null },
                { "max", this.MaxDate > DateTime.MinValue ? this.MaxDate.ToString(this.Format, CultureInfo.InvariantCulture) : null },
                { "data-client-format", this.ClientFormat },
            };
        }
    }
}
