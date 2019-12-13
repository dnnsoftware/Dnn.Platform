using System;

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowException : ApplicationException
    {
        public WorkflowException()
        {
            
        }

        public WorkflowException(string message) : base(message)
        {
            
        }

        public WorkflowException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
