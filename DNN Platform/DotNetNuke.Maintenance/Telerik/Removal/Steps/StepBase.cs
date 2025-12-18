// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <summary>A base class that implements <see cref="IStep"/>.</summary>
    internal abstract class StepBase : IStep
    {
        private readonly ILocalizer localizer;

        /// <summary>Initializes a new instance of the <see cref="StepBase"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        public StepBase(ILoggerSource loggerSource, ILocalizer localizer)
        {
            if (loggerSource is null)
            {
                throw new ArgumentNullException(nameof(loggerSource));
            }

            this.Log = loggerSource.GetLogger(this.GetType());

            this.localizer = localizer ??
                throw new ArgumentNullException(nameof(localizer));
        }

        /// <inheritdoc/>
        public virtual string Name { get; set; }

        /// <inheritdoc/>
        public bool? Success { get; protected set; }

        /// <inheritdoc/>
        public string Notes { get; protected set; }

        /// <inheritdoc/>
        public bool Quiet { get; protected set; }

        /// <summary>Gets an instance of <see cref="ILog"/> specific to steps.</summary>
        protected ILog Log { get; private set; }

        /// <inheritdoc/>
        public void Execute()
        {
            try
            {
                this.CheckRequired();
                this.ExecuteInternal();
            }
            catch (Exception ex)
            {
                this.Log.Error(ex);
                this.Success = false;
                this.Notes = this.Localize("UninstallStepInternalError");
            }
        }

        /// <summary>Performs the actual step execution.</summary>
        protected abstract void ExecuteInternal();

        /// <inheritdoc cref="ILocalizer.Localize(string)"/>
        protected virtual string Localize(string key)
        {
            return this.localizer.Localize(key);
        }

        /// <inheritdoc cref="ILocalizer.LocalizeFormat(string, object[])"/>
        protected virtual string LocalizeFormat(string key, params object[] args)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize(key),
                args);
        }

        private void CheckRequired()
        {
            var nullProperties = this.GetType().GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                .Where(p => string.IsNullOrEmpty($"{p.GetValue(this)}"))
                .Select(p => p.Name)
                .ToArray();

            if (nullProperties.Length > 0)
            {
                throw new InvalidOperationException(this.LocalizeFormat(
                    "UninstallStepRequiredPropertiesNotSet",
                    string.Join(", ", nullProperties)));
            }
        }
    }
}
