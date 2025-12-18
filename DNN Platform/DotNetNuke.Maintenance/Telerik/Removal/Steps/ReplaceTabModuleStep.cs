// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc />
    internal partial class ReplaceTabModuleStep : StepBase, IReplaceTabModuleStep, IStepArray
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IList<IReplacePortalTabModuleStep> steps;

        /// <summary>Initializes a new instance of the <see cref="ReplaceTabModuleStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public ReplaceTabModuleStep(ILoggerSource loggerSource, ILocalizer localizer, IServiceProvider serviceProvider)
            : base(loggerSource, localizer)
        {
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            this.steps = new List<IReplacePortalTabModuleStep>();
        }

        /// <inheritdoc />
        public override string Name => this.LocalizeFormat(
            "UninstallStepReplacePageModule",
            this.OldModuleName,
            this.NewModuleName);

        /// <inheritdoc />
        [Required]
        public string OldModuleName { get; set; }

        /// <inheritdoc />
        [Required]
        public string NewModuleName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IStep> Steps => this.steps;

        /// <inheritdoc/>
        public Func<Hashtable, Hashtable> MigrateSettings { get; set; }

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
            var portalController = this.GetService<IPortalController>();

            var steps = portalController.GetPortals()
                .Cast<IPortalInfo>()
                .Select(info =>
                {
                    var step = this.GetService<IReplacePortalTabModuleStep>();
                    step.ParentStep = this;
                    step.PortalId = info.PortalId;
                    return step;
                });

            foreach (var step in steps)
            {
                this.steps.Add(step);
                step.Execute();
            }
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
