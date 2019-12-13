#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Analytics.Config
{
    [Serializable]
    public class AnalyticsRule
    {
        private string _label;
        private int _roleId;
        private int _tabId;

        public int RoleId
        {
            get
            {
                return _roleId;
            }
            set
            {
                _roleId = value;
            }
        }

        public int TabId
        {
            get
            {
                return _tabId;
            }
            set
            {
                _tabId = value;
            }
        }

        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
            }
        }

        public string Value { get; set; }
    }
}
