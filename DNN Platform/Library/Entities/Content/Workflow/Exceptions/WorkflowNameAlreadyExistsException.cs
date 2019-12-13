using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowNameAlreadyExistsException : WorkflowException
    {
        public WorkflowNameAlreadyExistsException()
            : base(Localization.GetString("WorkflowNameAlreadyExistsException", Localization.ExceptionsResourceFile))
        {
            
        }
    }
}
