using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowDoesNotExistException : WorkflowException
    {
        public WorkflowDoesNotExistException()
            : base(Localization.GetString("WorkflowDoesNotExistException", Localization.ExceptionsResourceFile))
        {
            
        }
    }
}
