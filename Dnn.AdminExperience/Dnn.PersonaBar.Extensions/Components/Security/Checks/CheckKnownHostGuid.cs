// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Collections.Generic;

    using Dnn.PersonaBar.Security.Components;
    using DotNetNuke.Abstractions.Application;

    /// <summary>
    /// Checks and warns about using a known common Host GUID.
    /// </summary>
    public class CheckKnownHostGuid : IAuditCheck
    {
        private readonly IHostSettings hostSettings;

        private readonly bool hasKnownGuid;

        /// <summary>Initializes a new instance of the <see cref="CheckKnownHostGuid"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public CheckKnownHostGuid(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings;

            var knownGuids = new HashSet<string>(
                [
                    "181E4BC8-3FFE-4E30-8E08-038ACE517B97", // DNN 10.0.0 to 10.2.1
                ],
                StringComparer.OrdinalIgnoreCase);

            this.hasKnownGuid = knownGuids.Contains(this.hostSettings.Guid);
        }

        /// <inheritdoc/>
        public string Id => "CheckKnownHostGuid";

        /// <inheritdoc/>
        public bool LazyLoad => false;

        /// <summary>
        /// Gets a value indicating whether an alert should be triggered based on the current state.
        /// </summary>
        public bool ShouldAlert => this.hasKnownGuid;

        /// <inheritdoc/>
        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Pass, this.Id);

            if (this.hasKnownGuid)
            {
                result.Severity = SeverityEnum.Warning;
                result.Notes.Add(this.hostSettings.Guid);
            }

            return result;
        }
    }
}
