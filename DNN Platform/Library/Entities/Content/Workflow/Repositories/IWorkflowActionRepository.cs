using DotNetNuke.Entities.Content.Workflow.Actions;

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    /// <summary>
    /// This class is responsible to persist and retrieve workflow action entity
    /// </summary>
    internal interface IWorkflowActionRepository
    {
        /// <summary>
        /// This method gets the workflow action of a content type Id and action type
        /// </summary>
        /// <param name="contentTypeId">Content Item Id</param>
        /// <param name="actionType">Action type</param>
        /// <returns>Workflow action entity</returns>
        WorkflowAction GetWorkflowAction(int contentTypeId, string actionType);

        /// <summary>
        /// This method persists a new workflow action
        /// </summary>
        /// <param name="workflowAction">Workflow action entity</param>
        void AddWorkflowAction(WorkflowAction workflowAction);
    }
}
