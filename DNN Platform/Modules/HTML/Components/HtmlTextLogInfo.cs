using System;


namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextLogInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Defines an instance of an HtmlTextLog object
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class HtmlTextLogInfo
    {
        // local property declarations

        // initialization

        // public properties
        public int ItemID { get; set; }

        public int StateID { get; set; }

        public string StateName { get; set; }

        public string Comment { get; set; }

        public bool Approved { get; set; }

        public int CreatedByUserID { get; set; }

        public string DisplayName { get; set; }

        public DateTime CreatedOnDate { get; set; }
    }
}
