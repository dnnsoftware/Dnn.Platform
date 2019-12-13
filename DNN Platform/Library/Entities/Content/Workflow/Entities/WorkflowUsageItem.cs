using System;

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>
    /// This entity represents an item which is using a Workflow
    /// </summary>
    [Serializable]
    public class WorkflowUsageItem
    {
        /// <summary>
        /// Workflow Id
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Name of the item
        /// </summary>
        public string ContentName { get; set; }

        /// <summary>
        /// Type of the item
        /// </summary>
        public string ContentType { get; set; }
    }
}
