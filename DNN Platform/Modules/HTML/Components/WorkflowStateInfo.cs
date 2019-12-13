using System;

namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      WorkflowStateInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Defines an instance of a WorkflowState object
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class WorkflowStateInfo
    {
        // local property declarations

        private bool _IsActive = true;

        // public properties
        public int PortalID { get; set; }

        public int WorkflowID { get; set; }

        public string WorkflowName { get; set; }

        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public int StateID { get; set; }

        public string StateName { get; set; }

        public int Order { get; set; }

        public bool Notify { get; set; }

        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                _IsActive = value;
            }
        }
    }
}
