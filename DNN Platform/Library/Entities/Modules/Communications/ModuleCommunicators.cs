// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Communications
{
    using System.Collections;

    public class ModuleCommunicators : CollectionBase
    {
        public IModuleCommunicator this[int index]
        {
            get
            {
                return (IModuleCommunicator)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        public int Add(IModuleCommunicator item)
        {
            return this.List.Add(item);
        }
    }
}
