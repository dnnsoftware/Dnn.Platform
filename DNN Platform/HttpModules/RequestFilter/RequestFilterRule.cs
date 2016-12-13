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
#region Usings

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

#endregion

namespace DotNetNuke.HttpModules.RequestFilter
{
    [Serializable]
    public class RequestFilterRule
    {
        private RequestFilterRuleType _Action;
        private string _Location;
        private RequestFilterOperatorType _Operator;
        private string _ServerVariable;
        private List<string> _Values = new List<string>();

        /// <summary>
        /// Initializes a new instance of the RequestFilterRule class.
        /// </summary>
        /// <param name="serverVariable"></param>
        /// <param name="values"></param>
        /// <param name="op"></param>
        /// <param name="action"></param>
        /// <param name="location"></param>
        public RequestFilterRule(string serverVariable, string values, RequestFilterOperatorType op, RequestFilterRuleType action, string location)
        {
            _ServerVariable = serverVariable;
            SetValues(values, op);
            _Operator = op;
            _Action = action;
            _Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the RequestFilterRule class.
        /// </summary>
        public RequestFilterRule()
        {
        }

        public string ServerVariable
        {
            get
            {
                return _ServerVariable;
            }
            set
            {
                _ServerVariable = value;
            }
        }

        public List<string> Values
        {
            get
            {
                return _Values;
            }
            set
            {
                _Values = value;
            }
        }

        public string RawValue
        {
            get
            {
                return string.Join(" ", _Values.ToArray());
            }
        }

        public RequestFilterRuleType Action
        {
            get
            {
                return _Action;
            }
            set
            {
                _Action = value;
            }
        }

        public RequestFilterOperatorType Operator
        {
            get
            {
                return _Operator;
            }
            set
            {
                _Operator = value;
            }
        }

        public string Location
        {
            get
            {
                return _Location;
            }
            set
            {
                _Location = value;
            }
        }

        public void SetValues(string values, RequestFilterOperatorType op)
        {
            _Values.Clear();
            if ((op != RequestFilterOperatorType.Regex))
            {
                string[] vals = values.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string value in vals)
                {
                    _Values.Add(value.ToUpperInvariant());
                }
            }
            else
            {
                _Values.Add(values);
            }
        }

        public bool Matches(string ServerVariableValue)
        {
            switch (Operator)
            {
                case RequestFilterOperatorType.Equal:
                    return Values.Contains(ServerVariableValue.ToUpperInvariant());
                case RequestFilterOperatorType.NotEqual:
                    return !Values.Contains(ServerVariableValue.ToUpperInvariant());
                case RequestFilterOperatorType.Regex:
                    return Regex.IsMatch(ServerVariableValue, Values[0], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            return false;
        }

        public void Execute()
        {
            HttpResponse response = HttpContext.Current.Response;
            switch (Action)
            {
                case RequestFilterRuleType.Redirect:
                    response.Redirect(Location, true);
                    break;
                case RequestFilterRuleType.PermanentRedirect:
                    response.StatusCode = 301;
                    response.Status = "301 Moved Permanently";
                    response.RedirectLocation = Location;
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
