// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation.Runspaces;
using System.Management.Automation;

namespace DotNetNuke.Tests.UI.WatiN.Utilities
{
    public class PowershellManager
    {
        public static void ExecutePowershellScript(string scriptPath, string executionPolicy, params string[] parameters)
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();

            using (RunspaceInvoke invoker = new RunspaceInvoke())
            {
                invoker.Invoke(String.Format("Set-ExecutionPolicy {0}", executionPolicy));
                Command myCommand = new Command(scriptPath);
                for(int i=0; i < parameters.Length; i++)
                {
                    myCommand.Parameters.Add(new CommandParameter(null, parameters[i]));
                }

                Pipeline pipeline = runspace.CreatePipeline();
                pipeline.Commands.Add(myCommand);
                var results = pipeline.Invoke();
            }
        }
    }
}
