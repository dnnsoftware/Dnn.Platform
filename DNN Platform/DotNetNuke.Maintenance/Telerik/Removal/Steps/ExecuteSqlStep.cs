// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Data;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc cref="IExecuteSqlStep" />
    internal sealed class ExecuteSqlStep : StepBase, IExecuteSqlStep
    {
        private readonly IDataProvider dataProvider;

        /// <summary>Initializes a new instance of the <see cref="ExecuteSqlStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="dataProvider">An instance of <see cref="IDataProvider"/>.</param>
        public ExecuteSqlStep(ILoggerSource loggerSource, ILocalizer localizer, IDataProvider dataProvider)
            : base(loggerSource, localizer)
        {
            this.dataProvider = dataProvider ??
                throw new ArgumentNullException(nameof(dataProvider));
        }

        /// <inheritdoc/>
        [Required]
        public string CommandText { get; set; }

        /// <inheritdoc/>
        public IDataReader Result { get; private set; }

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            this.Result = this.dataProvider.ExecuteSQL(this.CommandText);
            this.Notes = this.LocalizeFormat("UninstallStepCountOfRecordsAffected", this.Result.RecordsAffected);
            this.Success = true;
        }
    }
}
