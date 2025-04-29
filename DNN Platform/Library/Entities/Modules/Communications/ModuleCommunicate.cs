// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Communications;

using System.Web.UI;

/// <summary>
/// Specifies communications between modules.
/// There are listeners and communicators.
/// </summary>
public class ModuleCommunicate
{
    private readonly ModuleCommunicators moduleCommunicators = new ModuleCommunicators();

    private readonly ModuleListeners moduleListeners = new ModuleListeners();

    /// <summary>Gets the module communicators.</summary>
    /// <value>
    /// The module communicators.
    /// </value>
    public ModuleCommunicators ModuleCommunicators
    {
        get
        {
            return this.moduleCommunicators;
        }
    }

    /// <summary>Gets the module listeners.</summary>
    /// <value>
    /// The module listeners.
    /// </value>
    public ModuleListeners ModuleListeners
    {
        get
        {
            return this.moduleListeners;
        }
    }

    /// <summary>Loads the communicator.</summary>
    /// <param name="ctrl">The control.</param>
    public void LoadCommunicator(Control ctrl)
    {
        // Check and see if the module implements IModuleCommunicator
        if (ctrl is IModuleCommunicator)
        {
            this.Add((IModuleCommunicator)ctrl);
        }

        // Check and see if the module implements IModuleListener
        if (ctrl is IModuleListener)
        {
            this.Add((IModuleListener)ctrl);
        }
    }

    private int Add(IModuleCommunicator item)
    {
        int returnData = this.moduleCommunicators.Add(item);

        int i = 0;
        for (i = 0; i <= this.moduleListeners.Count - 1; i++)
        {
            item.ModuleCommunication += this.moduleListeners[i].OnModuleCommunication;
        }

        return returnData;
    }

    private int Add(IModuleListener item)
    {
        int returnData = this.moduleListeners.Add(item);

        int i = 0;
        for (i = 0; i <= this.moduleCommunicators.Count - 1; i++)
        {
            this.moduleCommunicators[i].ModuleCommunication += item.OnModuleCommunication;
        }

        return returnData;
    }
}
