#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

    /// <summary>
    /// This event gets fired when any activity gets recorded
    /// </summary>
    public delegate void ActivityEventHandler(string status);

    /// <summary>
    /// Interface for an Installation Step
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public interface IInstallationStep
    {
        #region Properties

        /// <summary>
        /// Any details of the task while it's executing
        /// </summary>        
        string Details { get; }

        /// <summary>
        /// Percentage done
        /// </summary>        
        int Percentage { get; }

        /// <summary>
        /// Step Status
        /// </summary>        
        StepStatus Status { get; }

        /// <summary>
        /// List of Errors
        /// </summary>        
        IList<string> Errors { get; }

        #endregion

        #region Methods
        
        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        void Execute();

        #endregion

        #region Events

        /// <summary>
        /// This event gets fired when any activity gets recorded
        /// </summary>
        event ActivityEventHandler Activity;

        #endregion

    }
}
