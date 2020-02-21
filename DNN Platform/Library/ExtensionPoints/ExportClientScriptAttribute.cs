// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
