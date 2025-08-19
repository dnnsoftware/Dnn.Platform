﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvp
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>A view state serializer based on <see cref="ViewStateAttribute"/>.</summary>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial class AttributeBasedViewStateSerializer
    {
        private const BindingFlags MemberBindingFlags = BindingFlags.Instance | BindingFlags.Public;

        public static void DeSerialize(object value, StateBag state)
        {
            Type typ = value.GetType();

            // Parse all the Public instance properties
            foreach (PropertyInfo member in typ.GetProperties(MemberBindingFlags))
            {
                // Determine if they are attributed with a ViewState Attribute
                ViewStateAttribute attr = member.GetCustomAttributes(typeof(ViewStateAttribute), true).OfType<ViewStateAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    // Get object from ViewState bag
                    string viewStateKey = attr.ViewStateKey;
                    if (string.IsNullOrEmpty(viewStateKey))
                    {
                        // Use class member's name for Key
                        viewStateKey = member.Name;
                    }

                    member.SetValue(value, state[viewStateKey], null);
                }
            }
        }

        public static void Serialize(object value, StateBag state)
        {
            Type typ = value.GetType();

            // Parse all the Public instance properties
            foreach (PropertyInfo member in typ.GetProperties(MemberBindingFlags))
            {
                // Determine if they are attributed with a ViewState Attribute
                ViewStateAttribute attr = member.GetCustomAttributes(typeof(ViewStateAttribute), true).OfType<ViewStateAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    // Add property to ViewState bag
                    string viewStateKey = attr.ViewStateKey;
                    if (string.IsNullOrEmpty(viewStateKey))
                    {
                        // Use class member's name for Key
                        viewStateKey = member.Name;
                    }

                    state[viewStateKey] = member.GetValue(value, null);
                }
            }
        }
    }
}
