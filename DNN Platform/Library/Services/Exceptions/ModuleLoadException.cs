// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions;

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using DotNetNuke.Entities.Modules;

public class ModuleLoadException : BasePortalException
{
    private readonly ModuleInfo moduleConfiguration;
    private string friendlyName;
    private string moduleControlSource;
    private int moduleDefId;
    private int moduleId;

    // default constructor

    /// <summary>Initializes a new instance of the <see cref="ModuleLoadException"/> class.</summary>
    public ModuleLoadException()
    {
    }

    // constructor with exception message

    /// <summary>Initializes a new instance of the <see cref="ModuleLoadException"/> class.</summary>
    /// <param name="message"></param>
    public ModuleLoadException(string message)
        : base(message)
    {
        this.InitilizePrivateVariables();
    }

    // constructor with exception message

    /// <summary>Initializes a new instance of the <see cref="ModuleLoadException"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    /// <param name="moduleConfiguration"></param>
    public ModuleLoadException(string message, Exception inner, ModuleInfo moduleConfiguration)
        : base(message, inner)
    {
        this.moduleConfiguration = moduleConfiguration;
        this.InitilizePrivateVariables();
    }

    // constructor with message and inner exception

    /// <summary>Initializes a new instance of the <see cref="ModuleLoadException"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public ModuleLoadException(string message, Exception inner)
        : base(message, inner)
    {
        this.InitilizePrivateVariables();
    }

    /// <summary>Initializes a new instance of the <see cref="ModuleLoadException"/> class.</summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected ModuleLoadException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        this.InitilizePrivateVariables();
        this.moduleId = info.GetInt32("m_ModuleId");
        this.moduleDefId = info.GetInt32("m_ModuleDefId");
        this.friendlyName = info.GetString("m_FriendlyName");
    }

    [XmlElement("ModuleID")]
    public int ModuleId
    {
        get
        {
            return this.moduleId;
        }
    }

    [XmlElement("ModuleDefId")]
    public int ModuleDefId
    {
        get
        {
            return this.moduleDefId;
        }
    }

    [XmlElement("FriendlyName")]
    public string FriendlyName
    {
        get
        {
            return this.friendlyName;
        }
    }

    [XmlElement("ModuleControlSource")]
    public string ModuleControlSource
    {
        get
        {
            return this.moduleControlSource;
        }
    }

    private void InitilizePrivateVariables()
    {
        // Try and get the Portal settings from context
        // If an error occurs getting the context then set the variables to -1
        if (this.moduleConfiguration != null)
        {
            this.moduleId = this.moduleConfiguration.ModuleID;
            this.moduleDefId = this.moduleConfiguration.ModuleDefID;
            this.friendlyName = this.moduleConfiguration.ModuleTitle;
            this.moduleControlSource = this.moduleConfiguration.ModuleControl.ControlSrc;
        }
        else
        {
            this.moduleId = -1;
            this.moduleDefId = -1;
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
