// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Entities.Modules.Actions
{
    ///-----------------------------------------------------------------------------
    /// Project		: DotNetNuke
    /// Namespace   : DotNetNuke.Entities.Modules.Actions
    /// Class		: ActionEventArgs
    ///-----------------------------------------------------------------------------
    /// <summary>
    /// ActionEventArgs provides a custom EventARgs class for Action Events
    /// </summary>
    /// <remarks></remarks>
    ///-----------------------------------------------------------------------------
    public class ActionEventArgs : EventArgs
    {
        private readonly ModuleAction _action;
        private readonly ModuleInfo _moduleConfiguration;

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// </summary>
        /// <param name="Action"></param>
        /// <param name="ModuleConfiguration"></param>
        /// <remarks></remarks>
        ///-----------------------------------------------------------------------------
        public ActionEventArgs(ModuleAction Action, ModuleInfo ModuleConfiguration)
        {
            _action = Action;
            _moduleConfiguration = ModuleConfiguration;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        ///-----------------------------------------------------------------------------
        public ModuleAction Action
        {
            get
            {
                return _action;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        ///-----------------------------------------------------------------------------
        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return _moduleConfiguration;
            }
        }
    }
}
