// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.3.0. Please use TextBox with TextMode=TextBoxMode.DateTimeLocal. Scheduled removal in v12.0.0.")]
    public class DnnDateTimePicker : DnnDatePicker
    {
        /// <summary>Initializes a new instance of the <see cref="DnnDateTimePicker"/> class.</summary>
        public DnnDateTimePicker()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnDateTimePicker"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DnnDateTimePicker(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
            : base(
                clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>(),
                appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>())
        {
        }

        /// <inheritdoc />
        protected override string Format => "yyyy-MM-dd'T'HH:mm";

        /// <inheritdoc />
        protected override IDictionary<string, object> GetSettings()
        {
            var settings = base.GetSettings();

            settings["type"] = "datetime-local";

            return settings;
        }
    }
}
