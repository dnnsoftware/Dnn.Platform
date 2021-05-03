// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.UI.Modules;

    public class ModuleRequestResult
    {
        public ActionResult ActionResult
        {
            get => Task.Run(async () => await this.ActionResultTask).GetAwaiter().GetResult();
            set => this.ActionResultTask = Task.FromResult(value);
        }

        public Task<ActionResult> ActionResultTask { get; set; }

        public ControllerContext ControllerContext
        {
            get => Task.Run(async () => await this.ControllerContextTask).GetAwaiter().GetResult();
            set => this.ControllerContextTask = Task.FromResult(value);
        }

        public Task<ControllerContext> ControllerContextTask { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        public ModuleActionCollection ModuleActions
        {
            get => Task.Run(async () => await this.ModuleActionsTask).GetAwaiter().GetResult();
            set => this.ModuleActionsTask = Task.FromResult(value);
        }

        public Task<ModuleActionCollection> ModuleActionsTask { get; set; }

        public ModuleApplication ModuleApplication { get; set; }
    }
}
