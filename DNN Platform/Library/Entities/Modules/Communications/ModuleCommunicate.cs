// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
