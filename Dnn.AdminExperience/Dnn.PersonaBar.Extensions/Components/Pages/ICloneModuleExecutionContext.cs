// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    /// <summary>
    /// Provides an abstraction over the current context of the Clone Module thread.
    /// </summary>
    public interface ICloneModuleExecutionContext
    {
        /// <summary>
        /// Sets whether the import/export process of the Visualizer module is executed
        /// as part of the process of cloning a module or not. 
        /// F.i: when creating a page from a template, duplicating, etc.
        /// </summary>
        void SetCloneModuleContext(bool cloneModule);
    }
}
