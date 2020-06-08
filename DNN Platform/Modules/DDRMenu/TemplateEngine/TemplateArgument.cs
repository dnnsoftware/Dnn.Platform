// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    public class TemplateArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public TemplateArgument()
        {
        }

        public TemplateArgument(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
