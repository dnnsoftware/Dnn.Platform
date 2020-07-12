// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Tests.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public enum LoginAsUser
    {
        GivenUserName = 0,
        AnonymousUser,
        RegisteredUser,
        Host,
    }

    public abstract class WebApiExecuter
    {
        public Func<string, string, string, IWebApiConnector> Login;

        protected Func<string, IWebApiConnector> Anonymous;

        protected List<HttpResponseMessage> Responses = new List<HttpResponseMessage>();

        private LoginAsUser _loginAs;
        private IWebApiConnector _connector;

        protected WebApiExecuter()
        {
            this.LoginAs = LoginAsUser.RegisteredUser;
            this.Login = WebApiTestHelper.LoginRegisteredUser;
            this.Anonymous = WebApiTestHelper.GetAnnonymousConnector;

            this.UserFirstName = Constants.RuFirstName;
            this.UserLastName = Constants.RuLastName;
        }

        public string UserName
        {
            get
            {
                return this.Connector.UserName;
            }
        }

        public string DisplayName
        {
            get
            {
                return string.Join(" ", this.UserFirstName, this.UserLastName);
            }
        }

        public int UserId
        {
            get
            {
                return DatabaseHelper.ExecuteScalar<int>($"SELECT UserId FROM {{objectQualifier}}Users WHERE UserName = '{this.UserName}'");
            }
        }

        public LoginAsUser LoginAs
        {
            get
            {
                return this._loginAs;
            }

            set
            {
                this._loginAs = value;
                this._connector = null;
            }
        }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public IWebApiConnector Connector
        {
            get
            {
                if (this._connector == null)
                {
                    switch (this.LoginAs)
                    {
                        case LoginAsUser.RegisteredUser:
                            this._connector = this.Login(this.UserFirstName, this.UserLastName, null);
                            break;
                        case LoginAsUser.AnonymousUser:
                            this._connector = this.Anonymous(null);
                            break;
                        case LoginAsUser.Host:
                            this._connector = WebApiTestHelper.LoginHost();
                            break;
                        default:
                            this._connector = this.Login(this.UserFirstName, this.UserLastName, null);
                            break;
                    }
                }

                return this._connector;
            }

            set
            {
                this._connector = value;
                var userName = this._connector.UserName.Split('.');
                if (userName.Length == 2)
                {
                    this.UserFirstName = userName[0];
                    this.UserLastName = userName[1];
                }
            }
        }

        public HttpResponseMessage GetLastResponseMessage()
        {
            return this.Responses.Last();
        }

        /// <summary>
        /// Return the last executer's response deserialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the executer does not have any response.</exception>
        /// <returns></returns>
        public JContainer GetLastDeserializeResponseMessage()
        {
            if (!this.Responses.Any())
            {
                throw new InvalidOperationException("GetLastDeserializeResponseMessage cannot be called when the Executer does not have any Responses");
            }

            var data = this.Responses.Last().Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<JContainer>(data);
        }

        public IEnumerable<HttpResponseMessage> GetResponseMessages()
        {
            return this.Responses.ToArray();
        }

        /// <summary>
        /// Return the list of the executer's responses deserialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the executer does not have any response.</exception>
        /// <returns></returns>
        public IEnumerable<JContainer> GetDeserializeResponseMessages()
        {
            if (!this.Responses.Any())
            {
                throw new InvalidOperationException("GetDeserializeResponseMessages cannot be called when the Executer does not have any Responses");
            }

            return this.GetResponseMessages().Select(r => JsonConvert.DeserializeObject<JContainer>(r.Content.ReadAsStringAsync().Result));
        }
    }
}
