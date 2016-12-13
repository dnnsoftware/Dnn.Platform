#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
