// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Collections.Internal;

#endregion

namespace DotNetNuke.ComponentModel
{
    internal class ComponentBuilderCollection : SharedDictionary<string, IComponentBuilder>
    {
        internal IComponentBuilder DefaultBuilder { get; set; }

        internal void AddBuilder(IComponentBuilder builder, bool setDefault)
        {
            if (!ContainsKey(builder.Name))
            {
                this[builder.Name] = builder;

                if (setDefault && DefaultBuilder == null)
                {
                    DefaultBuilder = builder;
                }
            }
        }
    }
}
