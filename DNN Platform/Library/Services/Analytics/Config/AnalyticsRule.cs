// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics.Config
{
    using System;

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
                return this._roleId;
            }

            set
            {
                this._roleId = value;
            }
        }

        public int TabId
        {
            get
            {
                return this._tabId;
            }

            set
            {
                this._tabId = value;
            }
        }

        public string Label
        {
            get
            {
                return this._label;
            }

            set
            {
                this._label = value;
            }
        }

        public string Value { get; set; }
    }
}
