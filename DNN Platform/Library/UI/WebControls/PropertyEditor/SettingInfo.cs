// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      SettingInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SettingInfo class provides a helper class for the Settings Editor
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SettingInfo
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (SettingInfo));
        private Type _Type;

        public SettingInfo(object name, object value)
        {
            Name = Convert.ToString(name);
            Value = value;
            _Type = value.GetType();
            Editor = EditorInfo.GetEditor(-1);
            string strValue = Convert.ToString(value);
            bool IsFound = false;
            if (_Type.IsEnum)
            {
                IsFound = true;
            }
            if (!IsFound)
            {
                try
                {
                    bool boolValue = bool.Parse(strValue);
                    Editor = EditorInfo.GetEditor("Checkbox");
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
                    Editor = EditorInfo.GetEditor("Integer");
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
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }
    }
}
