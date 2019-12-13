using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowConcurrencyException : WorkflowException
    {
        public WorkflowConcurrencyException()
            : base(Localization.GetString("WorkflowConcurrencyException", Localization.ExceptionsResourceFile))
        {

        }
    }
}
