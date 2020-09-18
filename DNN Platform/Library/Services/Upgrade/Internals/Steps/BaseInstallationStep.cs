// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    using System;
    using System.Collections.Generic;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// BaseInstallationStep - Abstract class to perform common tasks for the various installation steps.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class BaseInstallationStep : IInstallationStep
    {
        protected string LocalInstallResourceFile = "~/Install/App_LocalResources/InstallWizard.aspx.resx";
        protected string LocalUpgradeResourceFile = "~/Install/App_LocalResources/UpgradeWizard.aspx.resx";
        private string _details = string.Empty;

        protected BaseInstallationStep()
        {
            this.Percentage = 0;
            this.Errors = new List<string>();
        }

        /// <summary>
        /// This event gets fired when any activity gets recorded
        /// </summary>
        public event ActivityEventHandler Activity;

        /// <summary>
        /// Gets or sets any details of the task while it's executing.
        /// </summary>
        public string Details
        {
            get
            {
                return this._details;
            }

            set
            {
                this._details = value;
                DnnInstallLogger.InstallLogInfo(this._details);
                if (this.Activity != null)
                {
                    this.Activity(this._details);
                }
            }
        }

        /// <summary>
        /// Gets or sets percentage done.
        /// </summary>
        public int Percentage { get; set; }

        /// <summary>
        /// Gets or sets step Status.
        /// </summary>
        public StepStatus Status { get; set; }

        /// <summary>
        /// Gets or sets list of Errors.
        /// </summary>
        public IList<string> Errors { get; set; }

        /// <summary>
        /// Main method to execute the step.
        /// </summary>
        public abstract void Execute();
    }
}
