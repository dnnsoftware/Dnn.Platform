// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml.Serialization;

    using DotNetNuke.Entities.Modules;

    public class ModuleLoadException : BasePortalException
    {
        private readonly ModuleInfo m_ModuleConfiguration;
        private string m_FriendlyName;
        private string m_ModuleControlSource;
        private int m_ModuleDefId;
        private int m_ModuleId;

        // default constructor
        public ModuleLoadException()
        {
        }

        // constructor with exception message
        public ModuleLoadException(string message)
            : base(message)
        {
            this.InitilizePrivateVariables();
        }

        // constructor with exception message
        public ModuleLoadException(string message, Exception inner, ModuleInfo ModuleConfiguration)
            : base(message, inner)
        {
            this.m_ModuleConfiguration = ModuleConfiguration;
            this.InitilizePrivateVariables();
        }

        // constructor with message and inner exception
        public ModuleLoadException(string message, Exception inner)
            : base(message, inner)
        {
            this.InitilizePrivateVariables();
        }

        protected ModuleLoadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.InitilizePrivateVariables();
            this.m_ModuleId = info.GetInt32("m_ModuleId");
            this.m_ModuleDefId = info.GetInt32("m_ModuleDefId");
            this.m_FriendlyName = info.GetString("m_FriendlyName");
        }

        [XmlElement("ModuleID")]
        public int ModuleId
        {
            get
            {
                return this.m_ModuleId;
            }
        }

        [XmlElement("ModuleDefId")]
        public int ModuleDefId
        {
            get
            {
                return this.m_ModuleDefId;
            }
        }

        [XmlElement("FriendlyName")]
        public string FriendlyName
        {
            get
            {
                return this.m_FriendlyName;
            }
        }

        [XmlElement("ModuleControlSource")]
        public string ModuleControlSource
        {
            get
            {
                return this.m_ModuleControlSource;
            }
        }

        private void InitilizePrivateVariables()
        {
            // Try and get the Portal settings from context
            // If an error occurs getting the context then set the variables to -1
            if (this.m_ModuleConfiguration != null)
            {
                this.m_ModuleId = this.m_ModuleConfiguration.ModuleID;
                this.m_ModuleDefId = this.m_ModuleConfiguration.ModuleDefID;
                this.m_FriendlyName = this.m_ModuleConfiguration.ModuleTitle;
                this.m_ModuleControlSource = this.m_ModuleConfiguration.ModuleControl.ControlSrc;
            }
            else
            {
                this.m_ModuleId = -1;
                this.m_ModuleDefId = -1;
            }
        }

        // public override void GetObjectData(SerializationInfo info, StreamingContext context)
        // {
        //    //Serialize this class' state and then call the base class GetObjectData
        //    info.AddValue("m_ModuleId", m_ModuleId, typeof (Int32));
        //    info.AddValue("m_ModuleDefId", m_ModuleDefId, typeof (Int32));
        //    info.AddValue("m_FriendlyName", m_FriendlyName, typeof (string));
        //    info.AddValue("m_ModuleControlSource", m_ModuleControlSource, typeof (string));
        //    base.GetObjectData(info, context);
        // }
    }
}
