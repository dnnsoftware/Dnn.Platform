// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Entities.Modules.Communications
{
    public class ModuleCommunicators : CollectionBase
    {
        public IModuleCommunicator this[int index]
        {
            get
            {
                return (IModuleCommunicator) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public int Add(IModuleCommunicator item)
        {
            return List.Add(item);
        }
    }
}
