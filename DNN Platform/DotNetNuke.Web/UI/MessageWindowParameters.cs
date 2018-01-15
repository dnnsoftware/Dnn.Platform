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