// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.RequestFilter
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Web;

    [Serializable]
    public class RequestFilterRule
    {
        private RequestFilterRuleType _Action;
        private string _Location;
        private RequestFilterOperatorType _Operator;
        private string _ServerVariable;
        private List<string> _Values = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFilterRule"/> class.
        /// </summary>
        /// <param name="serverVariable"></param>
        /// <param name="values"></param>
        /// <param name="op"></param>
        /// <param name="action"></param>
        /// <param name="location"></param>
        public RequestFilterRule(string serverVariable, string values, RequestFilterOperatorType op, RequestFilterRuleType action, string location)
        {
            this._ServerVariable = serverVariable;
            this.SetValues(values, op);
            this._Operator = op;
            this._Action = action;
            this._Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFilterRule"/> class.
        /// </summary>
        public RequestFilterRule()
        {
        }

        public string RawValue
        {
            get
            {
                return string.Join(" ", this._Values.ToArray());
            }
        }

        public string ServerVariable
        {
            get
            {
                return this._ServerVariable;
            }

            set
            {
                this._ServerVariable = value;
            }
        }

        public List<string> Values
        {
            get
            {
                return this._Values;
            }

            set
            {
                this._Values = value;
            }
        }

        public RequestFilterRuleType Action
        {
            get
            {
                return this._Action;
            }

            set
            {
                this._Action = value;
            }
        }

        public RequestFilterOperatorType Operator
        {
            get
            {
                return this._Operator;
            }

            set
            {
                this._Operator = value;
            }
        }

        public string Location
        {
            get
            {
                return this._Location;
            }

            set
            {
                this._Location = value;
            }
        }

        public void SetValues(string values, RequestFilterOperatorType op)
        {
            this._Values.Clear();
            if (op != RequestFilterOperatorType.Regex)
            {
                string[] vals = values.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string value in vals)
                {
                    this._Values.Add(value.ToUpperInvariant());
                }
            }
            else
            {
                this._Values.Add(values);
            }
        }

        public bool Matches(string ServerVariableValue)
        {
            switch (this.Operator)
            {
                case RequestFilterOperatorType.Equal:
                    return this.Values.Contains(ServerVariableValue.ToUpperInvariant());
                case RequestFilterOperatorType.NotEqual:
                    return !this.Values.Contains(ServerVariableValue.ToUpperInvariant());
                case RequestFilterOperatorType.Regex:
                    return Regex.IsMatch(ServerVariableValue, this.Values[0], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            return false;
        }

        public void Execute()
        {
            HttpResponse response = HttpContext.Current.Response;
            switch (this.Action)
            {
                case RequestFilterRuleType.Redirect:
                    response.Redirect(this.Location, true);
                    break;
                case RequestFilterRuleType.PermanentRedirect:
                    response.StatusCode = 301;
                    response.Status = "301 Moved Permanently";
                    response.RedirectLocation = this.Location;
                    response.End();
                    break;
                case RequestFilterRuleType.NotFound:
                    response.StatusCode = 404;
                    response.SuppressContent = true;
                    response.End();
                    break;
            }
        }
    }
}
