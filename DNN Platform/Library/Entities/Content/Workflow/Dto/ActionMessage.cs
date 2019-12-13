namespace DotNetNuke.Entities.Content.Workflow.Dto
{
    /// <summary>
    /// This dto class represents the message that will be sent as notification
    /// of a specific action on the Workflow (Complete/Discard state, Complete/Discard workflow)
    /// </summary>
    public class ActionMessage
    {
        /// <summary>
        /// Subject of the message
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body of the message
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Indicates if the message is going to be toasted or not. By default, it is False
        /// </summary>
        public bool SendToast { get; set; }
    }
}
