// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.MvcPipeline.Models
{
    public class RegisteredStylesheet
    {
        public string Stylesheet { get; set; }

        public FileOrder.Css FileOrder { get; set; }
    }
}
