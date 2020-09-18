// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    using DotNetNuke.Instrumentation;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      SettingInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SettingInfo class provides a helper class for the Settings Editor.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SettingInfo
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SettingInfo));
        private Type _Type;

        public SettingInfo(object name, object value)
        {
            this.Name = Convert.ToString(name);
            this.Value = value;
            this._Type = value.GetType();
            this.Editor = EditorInfo.GetEditor(-1);
            string strValue = Convert.ToString(value);
            bool IsFound = false;
            if (this._Type.IsEnum)
            {
                IsFound = true;
            }

            if (!IsFound)
            {
                try
                {
                    bool boolValue = bool.Parse(strValue);
                    this.Editor = EditorInfo.GetEditor("Checkbox");
                    IsFound = true;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }
            }

            if (!IsFound)
            {
                try
                {
                    int intValue = int.Parse(strValue);
                    this.Editor = EditorInfo.GetEditor("Integer");
                    IsFound = true;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }
            }
        }

        public string Name { get; set; }

        public object Value { get; set; }

        public string Editor { get; set; }

        public Type Type
        {
            get
            {
                return this._Type;
            }

            set
            {
                this._Type = value;
            }
        }
    }
}
