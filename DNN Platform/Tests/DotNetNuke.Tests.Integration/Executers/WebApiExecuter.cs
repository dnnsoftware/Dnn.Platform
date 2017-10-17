#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DotNetNuke.Tests.Integration.Framework;
using DotNetNuke.Tests.Integration.Framework.Helpers;
using DotNetNuke.Tests.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetNuke.Tests.Integration.Executers
{
    public enum LoginAsUser
    {
        GivenUserName = 0,
        AnonymousUser,
        RegisteredUser,
        Host
    };

    public abstract class WebApiExecuter
    {
        public Func<string, string, string, IWebApiConnector> Login;

        private LoginAsUser _loginAs;

        public LoginAsUser LoginAs
        {
            get
            {
                return _loginAs;
            }
            set
            {
                _loginAs = value;
                _connector = null;
            }
        }

        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }

        protected Func<string, IWebApiConnector> Anonymous;
        private IWebApiConnector _connector;

        protected WebApiExecuter()
        {
            LoginAs = LoginAsUser.RegisteredUser;
            Login = WebApiTestHelper.LoginRegisteredUser;
            Anonymous = WebApiTestHelper.GetAnnonymousConnector;

            UserFirstName = Constants.RuFirstName;
            UserLastName = Constants.RuLastName;
        }

        public IWebApiConnector Connector
        {
            get
            {
                if (_connector == null)
                {
                    switch (LoginAs)
                    {
                        case LoginAsUser.RegisteredUser:
                            _connector = Login(UserFirstName, UserLastName, null);
                            break;
                        case LoginAsUser.AnonymousUser:
                            _connector = Anonymous(null);
                            break;
                        case LoginAsUser.Host:
                            _connector = WebApiTestHelper.LoginHost();
                            break;
                        default:
                            _connector = Login(UserFirstName, UserLastName, null);
                            break;
                    }
                }
                return _connector;
            }
            set
            {
                _connector = value;
                var userName = _connector.UserName.Split('.');
                if (userName.Length == 2)
                {
                    UserFirstName = userName[0];
                    UserLastName = userName[1];
                }
            }
        }

        public string UserName
        {
            get
            {
                return Connector.UserName;
            }
        }

        public string DisplayName
        {
            get
            {
                return string.Join(" ", UserFirstName, UserLastName);
            }
        }

        public int UserId
        {
            get
            {
                return DatabaseHelper.ExecuteScalar<int>($"SELECT UserId FROM {{objectQualifier}}Users WHERE UserName = '{UserName}'");
            }
        }

        protected List<HttpResponseMessage> Responses = new List<HttpResponseMessage>();

        public HttpResponseMessage GetLastResponseMessage()
        {
            return Responses.Last();
        }

        /// <summary>
        /// Return the last executer's response deserialized 
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the executer does not have any response</exception>
        /// <returns></returns>
        public JContainer GetLastDeserializeResponseMessage()
        {
            if (!Responses.Any())
            {
                throw new InvalidOperationException("GetLastDeserializeResponseMessage cannot be called when the Executer does not have any Responses");
            }
            var data = Responses.Last().Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<JContainer>(data);
        }

        public IEnumerable<HttpResponseMessage> GetResponseMessages()
        {
            return Responses.ToArray();
        }

        /// <summary>
        /// Return the list of the executer's responses deserialized
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the executer does not have any response</exception>
        /// <returns></returns>
        public IEnumerable<JContainer> GetDeserializeResponseMessages()
        {
            if (!Responses.Any())
            {
                throw new InvalidOperationException("GetDeserializeResponseMessages cannot be called when the Executer does not have any Responses");
            }
            return GetResponseMessages().Select(r => JsonConvert.DeserializeObject<JContainer>(r.Content.ReadAsStringAsync().Result));
        }
    }
}
