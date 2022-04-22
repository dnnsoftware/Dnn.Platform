// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;

    /// <inheritdoc />
    internal partial class ReplaceTabModuleStep : StepBase, IReplaceTabModuleStep, IStepArray
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IList<IReplacePortalTabModuleStep> steps;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceTabModuleStep"/> class.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public ReplaceTabModuleStep(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            this.steps = new List<IReplacePortalTabModuleStep>();
        }

        /// <inheritdoc />
        public override string Name => string.Format(
            "Replace '{0}' with '{1}' in page '{2}'",
            this.OldModuleName,
            this.NewModuleName,
            this.PageName);

        /// <inheritdoc />
        public string PageName { get; set; }

        /// <inheritdoc />
        public string OldModuleName { get; set; }

        /// <inheritdoc />
        public string NewModuleName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IStep> Steps => this.steps;

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
            if (this.PageName is null)
            {
                throw new InvalidOperationException("Page name not set.");
            }

            if (this.OldModuleName is null)
            {
                throw new InvalidOperationException("Old module name not set.");
            }

            if (this.NewModuleName is null)
            {
                throw new InvalidOperationException("New module name not set.");
            }

            var portalController = this.GetService<IPortalController>();

            var steps = portalController.GetPortals()
                .Cast<IPortalInfo>()
                .Select(info =>
                {
                    var step = this.GetService<IReplacePortalTabModuleStep>();
                    step.ParentStep = this;
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
