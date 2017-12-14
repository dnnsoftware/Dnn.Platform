#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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