// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
