using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowStateNameAlreadyExistsException : WorkflowException
    {
        public WorkflowStateNameAlreadyExistsException()
            : base(Localization.GetString("WorkflowStateNameAlreadyExistsException", Localization.ExceptionsResourceFile))
        {
            
        }
    }
}
