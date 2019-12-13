﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI
{
    public class MessageWindowParameters
    {
        private string _Message = "";
        private string _Title = "";
        private Unit _WindowHeight = Unit.Pixel(175);
        private Unit _WindowWidth = Unit.Pixel(350);

        public MessageWindowParameters(string message)
        {
            _Message = message;
        }

        public MessageWindowParameters(string message, string title)
        {
            _Message = message;
            _Title = title;
        }

        public MessageWindowParameters(string message, string title, string windowWidth, string windowHeight)
        {
            _Message = message;
            _Title = title;
            _WindowWidth = Unit.Parse(windowWidth);
            _WindowHeight = Unit.Parse(windowHeight);
        }

        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                //todo: javascript encode for onclick events
                _Message = value;
                _Message = _Message.Replace("'", "\\'");
                _Message = _Message.Replace("\"", "\\\"");
            }
        }

        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                //todo: javascript encode for onclick events
                _Title = value;
                _Title = _Title.Replace("'", "\\'");
                _Title = _Title.Replace("\"", "\\\"");
            }
        }

        public Unit WindowWidth
        {
            get
            {
                return _WindowWidth;
            }
            set
            {
                _WindowWidth = value;
            }
        }

        public Unit WindowHeight
        {
            get
            {
                return _WindowHeight;
            }
            set
            {
                _WindowHeight = value;
            }
        }
    }
}
