// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DNN.Integration.Test.Framework;
using DNN.Integration.Test.Framework.Helpers;
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
