// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Threading;

    using DotNetNuke.Framework;

    public class CloneModuleExecutionContext : ServiceLocator<ICloneModuleExecutionContext, CloneModuleExecutionContext>, ICloneModuleExecutionContext
    {
        private const string CloneModuleSlotName = "CloneModuleContext";

        public void SetCloneModuleContext(bool cloneModule)
        {
            var slot = Thread.GetNamedDataSlot(CloneModuleSlotName);
            Thread.SetData(slot, cloneModule ? bool.TrueString : bool.FalseString);
        }

        protected override Func<ICloneModuleExecutionContext> GetFactory()
        {
            return () => new CloneModuleExecutionContext();
        }
    }
}
