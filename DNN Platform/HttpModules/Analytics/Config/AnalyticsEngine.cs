// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

#endregion

namespace DotNetNuke.HttpModules.Config
{
    [Serializable]
    public class AnalyticsEngine
    {
        private string _elementId;
        private string _engineType;
        private bool _injectTop;
        private string _scriptTemplate;

        public string EngineType
        {
            get
            {
                return _engineType;
            }
            set
            {
                _engineType = value;
            }
        }

        public string ScriptTemplate
        {
            get
            {
                return _scriptTemplate;
            }
            set
            {
                _scriptTemplate = value;
            }
        }

        public string ElementId
        {
            get
            {
                return _elementId;
            }
            set
            {
                _elementId = value;
            }
        }

        public bool InjectTop
        {
            get
            {
                return _injectTop;
            }
            set
            {
                _injectTop = value;
            }
        }
    }
}
