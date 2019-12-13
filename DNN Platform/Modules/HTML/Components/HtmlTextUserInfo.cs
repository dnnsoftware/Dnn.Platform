using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;


namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextUserInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Defines an instance of an HtmlTextUser object
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class HtmlTextUserInfo
    {
        #region Private Member
        // local property declarations
        private ModuleInfo _Module;

        #endregion

        #region Public Properties
        public int ItemID { get; set; }

        public int StateID { get; set; }

        public string StateName { get; set; }

        public int ModuleID { get; set; }

        public string ModuleTitle
        {
            get
            {
                string _ModuleTitle = Null.NullString;
                if (Module != null)
                {
                    _ModuleTitle = Module.ModuleTitle;
                }
                return _ModuleTitle;
            }
        }

        public ModuleInfo Module
        {
            get
            {
                if (_Module == null)
                {
                    _Module = ModuleController.Instance.GetModule(ModuleID, TabID, false);
                }
                return _Module;
            }
        }


        public int TabID { get; set; }

        public int UserID { get; set; }

        public DateTime CreatedOnDate { get; set; }
        #endregion
    }
}
