// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.IO;
using System.Web.Mvc;

namespace DotNetNuke.Web.Mvc.Framework.ActionResults
{
    public interface IDnnViewResult
    {
        void ExecuteResult(ControllerContext context, TextWriter writer);
    }
}
