// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.RequestFilter;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

[Serializable]
public class RequestFilterRule
{
    private RequestFilterRuleType action;
    private string location;
    private RequestFilterOperatorType @operator;
    private string serverVariable;
    private List<string> values = new List<string>();

    /// <summary>Initializes a new instance of the <see cref="RequestFilterRule"/> class.</summary>
    /// <param name="serverVariable"></param>
    /// <param name="values"></param>
    /// <param name="op"></param>
    /// <param name="action"></param>
    /// <param name="location"></param>
    public RequestFilterRule(string serverVariable, string values, RequestFilterOperatorType op, RequestFilterRuleType action, string location)
    {
        this.serverVariable = serverVariable;
        this.SetValues(values, op);
        this.@operator = op;
        this.action = action;
        this.location = location;
    }

    /// <summary>Initializes a new instance of the <see cref="RequestFilterRule"/> class.</summary>
    public RequestFilterRule()
    {
    }

    public string RawValue
    {
        get
        {
            return string.Join(" ", this.values.ToArray());
        }
    }

    public string ServerVariable
    {
        get
        {
            return this.serverVariable;
        }

        set
        {
            this.serverVariable = value;
        }
    }

    public List<string> Values
    {
        get
        {
            return this.values;
        }

        set
        {
            this.values = value;
        }
    }

    public RequestFilterRuleType Action
    {
        get
        {
            return this.action;
        }

        set
        {
            this.action = value;
        }
    }

    public RequestFilterOperatorType Operator
    {
        get
        {
            return this.@operator;
        }

        set
        {
            this.@operator = value;
        }
    }

    public string Location
    {
        get
        {
            return this.location;
        }

        set
        {
            this.location = value;
        }
    }

    public void SetValues(string values, RequestFilterOperatorType op)
    {
        this.values.Clear();
        if (op != RequestFilterOperatorType.Regex)
        {
            string[] vals = values.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in vals)
            {
                this.values.Add(value.ToUpperInvariant());
            }
        }
        else
        {
            this.values.Add(values);
        }
    }

    public bool Matches(string serverVariableValue)
    {
        switch (this.Operator)
        {
            case RequestFilterOperatorType.Equal:
                return this.Values.Contains(serverVariableValue.ToUpperInvariant());
            case RequestFilterOperatorType.NotEqual:
                return !this.Values.Contains(serverVariableValue.ToUpperInvariant());
            case RequestFilterOperatorType.Regex:
                return Regex.IsMatch(serverVariableValue, this.Values[0], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
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
