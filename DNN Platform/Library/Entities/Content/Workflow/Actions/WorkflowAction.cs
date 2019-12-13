using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    /// <summary>
    /// This entity represents a workflow action implementation
    /// </summary>
    [PrimaryKey("ActionId")]
    [TableName("ContentWorkflowActions")]
    public class WorkflowAction
    {
        /// <summary>
        /// Action Id
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Content item type Id
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// Action type. This is a string representation of the enum <see cref="WorkflowActionTypes"/>
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Action Source. This property represents the path to the class that implement the IWorkflowAction interface
        /// i.e.: "MyProject.WorkflowActions.WorkflowDiscardction, MyProject"
        /// </summary>
        public string ActionSource { get; set; }
    }
}
