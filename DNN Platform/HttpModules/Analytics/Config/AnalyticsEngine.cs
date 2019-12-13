// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
