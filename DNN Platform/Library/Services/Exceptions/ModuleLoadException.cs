#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml.Serialization;

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Exceptions
{
    public class ModuleLoadException : BasePortalException
    {
        private readonly ModuleInfo m_ModuleConfiguration;
        private string m_FriendlyName;
        private string m_ModuleControlSource;
        private int m_ModuleDefId;
        private int m_ModuleId;

        //default constructor
		public ModuleLoadException()
        {
        }

        //constructor with exception message
        public ModuleLoadException(string message) : base(message)
        {
            InitilizePrivateVariables();
        }

        //constructor with exception message
        public ModuleLoadException(string message, Exception inner, ModuleInfo ModuleConfiguration) : base(message, inner)
        {
            m_ModuleConfiguration = ModuleConfiguration;
            InitilizePrivateVariables();
        }

        //constructor with message and inner exception
        public ModuleLoadException(string message, Exception inner) : base(message, inner)
        {
            InitilizePrivateVariables();
        }

        protected ModuleLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            InitilizePrivateVariables();
            m_ModuleId = info.GetInt32("m_ModuleId");
            m_ModuleDefId = info.GetInt32("m_ModuleDefId");
            m_FriendlyName = info.GetString("m_FriendlyName");
        }

        [XmlElement("ModuleID")]
        public int ModuleId
        {
            get
            {
                return m_ModuleId;
            }
        }

        [XmlElement("ModuleDefId")]
        public int ModuleDefId
        {
            get
            {
                return m_ModuleDefId;
            }
        }

        [XmlElement("FriendlyName")]
        public string FriendlyName
        {
            get
            {
                return m_FriendlyName;
            }
        }

        [XmlElement("ModuleControlSource")]
        public string ModuleControlSource
        {
            get
            {
                return m_ModuleControlSource;
            }
        }

        private void InitilizePrivateVariables()
        {
			//Try and get the Portal settings from context
            //If an error occurs getting the context then set the variables to -1
            if ((m_ModuleConfiguration != null))
            {
                m_ModuleId = m_ModuleConfiguration.ModuleID;
                m_ModuleDefId = m_ModuleConfiguration.ModuleDefID;
                m_FriendlyName = m_ModuleConfiguration.ModuleTitle;
                m_ModuleControlSource = m_ModuleConfiguration.ModuleControl.ControlSrc;
            }
            else
            {
                m_ModuleId = -1;
                m_ModuleDefId = -1;
            }
        }

        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    //Serialize this class' state and then call the base class GetObjectData
        //    info.AddValue("m_ModuleId", m_ModuleId, typeof (Int32));
        //    info.AddValue("m_ModuleDefId", m_ModuleDefId, typeof (Int32));
        //    info.AddValue("m_FriendlyName", m_FriendlyName, typeof (string));
        //    info.AddValue("m_ModuleControlSource", m_ModuleControlSource, typeof (string));
        //    base.GetObjectData(info, context);
        //}
    }
}