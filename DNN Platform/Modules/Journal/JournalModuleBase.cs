// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
{
    /*
' Copyright (c) 2010 DotNetNuke Corporation
'  All rights reserved.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
'
*/

    using System;

    using DotNetNuke.Modules.Journal.Components;

    public class JournalModuleBase : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        public enum JournalMode
        {
            Auto = 0,
            Profile = 1,
            Group = 2,
        }

        public JournalMode FilterMode
        {
            get
            {
                if (!this.Settings.ContainsKey(Constants.JournalFilterMode))
                {
                    return JournalMode.Auto;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.Settings[Constants.JournalFilterMode].ToString()))
                    {
                        return JournalMode.Auto;
                    }
                    else
                    {
                        return (JournalMode)Convert.ToInt16(this.Settings[Constants.JournalFilterMode].ToString());
                    }
                }
            }
        }

        public int GroupId
        {
            get
            {
                int groupId = -1;
                if (!string.IsNullOrEmpty(this.Request.QueryString["groupid"]))
                {
                    if (int.TryParse(this.Request.QueryString["groupid"], out groupId))
                    {
                        return groupId;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }

        public bool EditorEnabled
        {
            get
            {
                if (!this.Settings.ContainsKey(Constants.JournalEditorEnabled))
                {
                    return true;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.Settings[Constants.JournalEditorEnabled].ToString()))
                    {
                        return true;
                    }
                    else
                    {
                        return (bool)Convert.ToBoolean(this.Settings[Constants.JournalEditorEnabled].ToString());
                    }
                }
            }
        }
    }
}
