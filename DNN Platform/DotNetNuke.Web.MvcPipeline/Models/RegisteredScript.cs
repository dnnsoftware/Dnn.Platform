// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Models
{
    public class RegisteredScript
    {
        public string Script { get; set; }

        public DotNetNuke.Web.Client.FileOrder.Js FileOrder { get; set; } = Client.FileOrder.Js.DefaultPriority;
    }
}
