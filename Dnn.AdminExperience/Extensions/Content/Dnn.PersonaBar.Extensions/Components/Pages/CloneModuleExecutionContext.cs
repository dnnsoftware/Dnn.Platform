// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Threading;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.Pages.Components
{
    public class CloneModuleExecutionContext : ServiceLocator<ICloneModuleExecutionContext, CloneModuleExecutionContext>, ICloneModuleExecutionContext
    {
        private const string CloneModuleSlotName = "CloneModuleContext";

        protected override Func<ICloneModuleExecutionContext> GetFactory()
        {
            return () => new CloneModuleExecutionContext();
        }

        public void SetCloneModuleContext(bool cloneModule)
        {
            var slot = Thread.GetNamedDataSlot(CloneModuleSlotName);
            Thread.SetData(slot, cloneModule ? bool.TrueString : bool.FalseString);
        }
    }
}
