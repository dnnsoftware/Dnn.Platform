// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionResults
{
    using System.IO;
    using System.Web.Mvc;

    public interface IDnnViewResult
    {
        void ExecuteResult(ControllerContext context, TextWriter writer);
    }
}
