// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Config;

using System;

[Serializable]
public class AnalyticsEngine
{
    private string elementId;
    private string engineType;
    private bool injectTop;
    private string scriptTemplate;

    public string EngineType
    {
        get
        {
            return this.engineType;
        }

        set
        {
            this.engineType = value;
        }
    }

    public string ScriptTemplate
    {
        get
        {
            return this.scriptTemplate;
        }

        set
        {
            this.scriptTemplate = value;
        }
    }

    public string ElementId
    {
        get
        {
            return this.elementId;
        }

        set
        {
            this.elementId = value;
        }
    }

    public bool InjectTop
    {
        get
        {
            return this.injectTop;
        }

        set
        {
            this.injectTop = value;
        }
    }
}
