// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Maintenance.Telerik.Steps;

    /// <summary>A data transfer object with an uninstallation summary item.</summary>
    public class UninstallSummaryItem
    {
        /// <summary>Initializes a new instance of the <see cref="UninstallSummaryItem"/> class.</summary>
        /// <param name="step">The step to summarize.</param>
        internal UninstallSummaryItem(IStep step)
        {
            this.Notes = step.Notes;
            this.StepName = step.Name;
            this.Success = step.Success;
        }

        /// <summary>Gets any remarks collected during step execution.</summary>
        public string Notes { get; private set; }

        /// <summary>Gets a value indicating whether the step succeeded, failed or was not executed.</summary>
        public bool? Success { get; private set; }

        /// <summary>Gets the step name.</summary>
        public string StepName { get; private set; }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of type <see cref="UninstallSummaryItem"/>
        /// containing the summary of the given <see cref="IStep"/> instance execution.
        /// </summary>
        /// <param name="step">The step to summarize.</param>
        /// <returns>
        /// A <see cref="UninstallSummaryItem"/> with data from the given <see cref="IStep"/> instance.
        /// </returns>
        internal static IEnumerable<UninstallSummaryItem> FromStep(IStep step)
        {
            if (step.Quiet && step.Success.HasValue && step.Success.Value)
            {
                return [];
            }

            return step is IStepArray stepArray && stepArray.Steps.Any()
                ? stepArray.Steps.SelectMany(s => FromStep(s)).ToArray()
                : [new UninstallSummaryItem(step)];
        }
    }
}
