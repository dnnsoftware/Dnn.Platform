// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Actions
{
    /// -----------------------------------------------------------------------------
    /// Project     : DotNetNuke
    /// Class       : ModuleActionEventListener
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public class ModuleActionEventListener
    {
        private readonly ActionEventHandler _actionEvent;
        private readonly int _moduleID;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleActionEventListener"/> class.
        /// </summary>
        /// <param name="ModID"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public ModuleActionEventListener(int ModID, ActionEventHandler e)
        {
            this._moduleID = ModID;
            this._actionEvent = e;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///
        /// </summary>
        /// <value>
        ///
        /// </value>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public int ModuleID
        {
            get
            {
                return this._moduleID;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///
        /// </summary>
        /// <value>
        ///
        /// </value>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public ActionEventHandler ActionEvent
        {
            get
            {
                return this._actionEvent;
            }
        }
    }
}
