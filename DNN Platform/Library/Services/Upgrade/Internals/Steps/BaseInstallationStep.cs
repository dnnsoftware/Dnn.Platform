#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// BaseInstallationStep - Abstract class to perform common tasks for the various installation steps
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public abstract class BaseInstallationStep : IInstallationStep
    {
        #region Protected

        protected string LocalInstallResourceFile = "~/Install/App_LocalResources/InstallWizard.aspx.resx";
        protected string LocalUpgradeResourceFile = "~/Install/App_LocalResources/UpgradeWizard.aspx.resx";

        #endregion

        #region Private

        private string _details = string.Empty;

        #endregion

        protected BaseInstallationStep()
        {
            Percentage = 0;
            Errors = new List<string>();
        }        

        #region Implementation of IInstallationStep

        /// <summary>
        /// Any details of the task while it's executing
        /// </summary>        
        public string Details
        {
            get
            {
                return _details;
            }
            set
            {
                _details = value;
				DnnInstallLogger.InstallLogInfo(_details);
                if (Activity != null)
                    Activity(_details);
            }
        }

        /// <summary>
        /// Percentage done
        /// </summary>        
        public int Percentage { get; set; }

        /// <summary>
        /// Step Status
        /// </summary>        
        public StepStatus Status { get; set; }

        /// <summary>
        /// List of Errors
        /// </summary>        
        public IList<string> Errors { get; set; }

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public abstract void Execute();

        /// <summary>
        /// This event gets fired when any activity gets recorded
        /// </summary>
        public event ActivityEventHandler Activity;

        #endregion
    }
}
