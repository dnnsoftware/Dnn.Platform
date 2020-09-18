// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System.ComponentModel.Composition;

    [MetadataAttribute]
    public class ExportClientScriptAttribute : ExportAttribute
    {
        public ExportClientScriptAttribute()
            : base(typeof(IScriptItemExtensionPoint))
        {
        }
    }
}
