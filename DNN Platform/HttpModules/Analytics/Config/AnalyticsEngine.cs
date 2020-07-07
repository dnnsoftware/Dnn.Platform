// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Config
{
    using System;

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
                return this._engineType;
            }

            set
            {
                this._engineType = value;
            }
        }

        public string ScriptTemplate
        {
            get
            {
                return this._scriptTemplate;
            }

            set
            {
                this._scriptTemplate = value;
            }
        }

        public string ElementId
        {
            get
            {
                return this._elementId;
            }

            set
            {
                this._elementId = value;
            }
        }

        public bool InjectTop
        {
            get
            {
                return this._injectTop;
            }

            set
            {
                this._injectTop = value;
            }
        }
    }
}
