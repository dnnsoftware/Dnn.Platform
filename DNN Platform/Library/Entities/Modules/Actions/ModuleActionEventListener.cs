// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Modules.Actions
{
    ///-----------------------------------------------------------------------------
    /// Project		: DotNetNuke
    /// Class		: ModuleActionEventListener
    ///
    ///-----------------------------------------------------------------------------
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    ///-----------------------------------------------------------------------------
    public class ModuleActionEventListener
    {
        private readonly ActionEventHandler _actionEvent;
        private readonly int _moduleID;

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModID"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        ///-----------------------------------------------------------------------------
        public ModuleActionEventListener(int ModID, ActionEventHandler e)
        {
            _moduleID = ModID;
            _actionEvent = e;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        ///-----------------------------------------------------------------------------
        public int ModuleID
        {
            get
            {
                return _moduleID;
            }
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        ///-----------------------------------------------------------------------------
        public ActionEventHandler ActionEvent
        {
            get
            {
                return _actionEvent;
            }
        }
    }
}
