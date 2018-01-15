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

using System.Web.UI;

#endregion

namespace DotNetNuke.Entities.Modules.Communications
{
    /// <summary>
    /// Specifies communications between modules. 
    /// There are listeners and communicators
    /// </summary>
    public class ModuleCommunicate
    {
        private readonly ModuleCommunicators _ModuleCommunicators = new ModuleCommunicators();

        private readonly ModuleListeners _ModuleListeners = new ModuleListeners();

        /// <summary>
        /// Gets the module communicators.
        /// </summary>
        /// <value>
        /// The module communicators.
        /// </value>
        public ModuleCommunicators ModuleCommunicators
        {
            get
            {
                return _ModuleCommunicators;
            }
        }

        /// <summary>
        /// Gets the module listeners.
        /// </summary>
        /// <value>
        /// The module listeners.
        /// </value>
        public ModuleListeners ModuleListeners
        {
            get
            {
                return _ModuleListeners;
            }
        }


        /// <summary>
        /// Loads the communicator.
        /// </summary>
        /// <param name="ctrl">The control.</param>
        public void LoadCommunicator(Control ctrl)
        {
            // Check and see if the module implements IModuleCommunicator 
            if (ctrl is IModuleCommunicator)
            {
                Add((IModuleCommunicator) ctrl);
            }

            // Check and see if the module implements IModuleListener 
            if (ctrl is IModuleListener)
            {
                Add((IModuleListener) ctrl);
            }
        }

        private int Add(IModuleCommunicator item)
        {
            int returnData = _ModuleCommunicators.Add(item);

            int i = 0;
            for (i = 0; i <= _ModuleListeners.Count - 1; i++)
            {
                item.ModuleCommunication += _ModuleListeners[i].OnModuleCommunication;
            }


            return returnData;
        }

        private int Add(IModuleListener item)
        {
            int returnData = _ModuleListeners.Add(item);

            int i = 0;
            for (i = 0; i <= _ModuleCommunicators.Count - 1; i++)
            {
                _ModuleCommunicators[i].ModuleCommunication += item.OnModuleCommunication;
            }

            return returnData;
        }
    }
}