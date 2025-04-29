// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.DependencyInjection;

using System.Web.Http.Filters;

using DotNetNuke.Abstractions.Logging;
using DotNetNuke.DependencyInjection;

public partial class BuildUpExtensionsTests
{
    private class PrivateFilterAttribute : ActionFilterAttribute
    {
        [Dependency]
        private IEventLogger EventLogger { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            this.EventLogger.AddLog("Test", "Message", EventLogType.ADMIN_ALERT);
            base.OnActionExecuted(actionExecutedContext);
        }
    }

    private class ProtectedFilterAttribute : ActionFilterAttribute
    {
        [Dependency]
        protected IEventLogger EventLogger { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            this.EventLogger.AddLog("Test", "Message", EventLogType.ADMIN_ALERT);
            base.OnActionExecuted(actionExecutedContext);
        }
    }

    private class ProtectedInternalFilterAttribute : ActionFilterAttribute
    {
        [Dependency]
        protected internal IEventLogger EventLogger { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            this.EventLogger.AddLog("Test", "Message", EventLogType.ADMIN_ALERT);
            base.OnActionExecuted(actionExecutedContext);
        }
    }

    private class InternalFilterAttribute : ActionFilterAttribute
    {
        [Dependency]
        internal IEventLogger EventLogger { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            this.EventLogger.AddLog("Test", "Message", EventLogType.ADMIN_ALERT);
            base.OnActionExecuted(actionExecutedContext);
        }
    }

    private class PublicFilterAttribute : ActionFilterAttribute
    {
        [Dependency]
        public IEventLogger EventLogger { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            this.EventLogger.AddLog("Test", "Message", EventLogType.ADMIN_ALERT);
            base.OnActionExecuted(actionExecutedContext);
        }
    }

    private class PublicNoDependencyAttributeFilterAttribute : ActionFilterAttribute
    {
        public IEventLogger EventLogger { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            // This should throw an exception as EventLogger will be null
            this.EventLogger.AddLog("Test", "Message", EventLogType.ADMIN_ALERT);
            base.OnActionExecuted(actionExecutedContext);
        }
    }

    private class NoSetFilterAttribute : ActionFilterAttribute
    {
        [Dependency]
        public IEventLogger EventLogger { get; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            this.EventLogger.AddLog("Test", "Message", EventLogType.ADMIN_ALERT);
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
