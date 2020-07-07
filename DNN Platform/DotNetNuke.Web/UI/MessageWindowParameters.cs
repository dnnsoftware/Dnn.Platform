// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI
{
    using System.Web.UI.WebControls;

    public class MessageWindowParameters
    {
        private string _Message = string.Empty;
        private string _Title = string.Empty;
        private Unit _WindowHeight = Unit.Pixel(175);
        private Unit _WindowWidth = Unit.Pixel(350);

        public MessageWindowParameters(string message)
        {
            this._Message = message;
        }

        public MessageWindowParameters(string message, string title)
        {
            this._Message = message;
            this._Title = title;
        }

        public MessageWindowParameters(string message, string title, string windowWidth, string windowHeight)
        {
            this._Message = message;
            this._Title = title;
            this._WindowWidth = Unit.Parse(windowWidth);
            this._WindowHeight = Unit.Parse(windowHeight);
        }

        public string Message
        {
            get
            {
                return this._Message;
            }

            set
            {
                // todo: javascript encode for onclick events
                this._Message = value;
                this._Message = this._Message.Replace("'", "\\'");
                this._Message = this._Message.Replace("\"", "\\\"");
            }
        }

        public string Title
        {
            get
            {
                return this._Title;
            }

            set
            {
                // todo: javascript encode for onclick events
                this._Title = value;
                this._Title = this._Title.Replace("'", "\\'");
                this._Title = this._Title.Replace("\"", "\\\"");
            }
        }

        public Unit WindowWidth
        {
            get
            {
                return this._WindowWidth;
            }

            set
            {
                this._WindowWidth = value;
            }
        }

        public Unit WindowHeight
        {
            get
            {
                return this._WindowHeight;
            }

            set
            {
                this._WindowHeight = value;
            }
        }
    }
}
