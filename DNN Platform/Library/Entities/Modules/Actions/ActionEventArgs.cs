#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
    /// <history>
    /// 	[Joe] 	10/26/2003	Created
    /// </history>
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
        /// <history>
        /// 	[Joe] 	10/26/2003	Created
        /// </history>
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
        /// <history>
        /// 	[Joe] 	10/26/2003	Created
        /// </history>
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
        /// <history>
        /// 	[jbrinkman] 	12/27/2003	Created
        /// </history>
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
