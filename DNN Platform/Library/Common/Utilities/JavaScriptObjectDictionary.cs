// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

public class JavaScriptObjectDictionary : IEnumerable<KeyValuePair<string, string>>
{
    private OrderedDictionary _dictionary = null;

    internal OrderedDictionary Dictionary
    {
        get
        {
            return this._dictionary ?? (this._dictionary = new OrderedDictionary());
        }
    }

    public static string ToJavaScriptArrayString(IEnumerable<KeyValuePair<string, string>> methods)
    {
        if (methods == null)
        {
            return "null";
        }

        var builder = new StringBuilder();
        builder.Append('[');
        var isFirstPair = true;
        foreach (var keyValuePair in methods)
        {
            if (isFirstPair)
            {
                isFirstPair = false;
            }
            else
            {
                builder.Append(',');
            }

            var methodValue = string.IsNullOrEmpty(keyValuePair.Value) ? "null" : keyValuePair.Value;
            builder.Append(methodValue);
        }

        builder.Append(']');
        return builder.ToString();
    }

    public static string ToJavaScriptArrayString(IEnumerable<string> methods)
    {
        if (methods == null)
        {
            return "null";
        }

        var builder = new StringBuilder();
        builder.Append('[');
        var isFirstPair = true;
        foreach (var method in methods)
        {
            if (isFirstPair)
            {
                isFirstPair = false;
            }
            else
            {
                builder.Append(',');
            }

            var methodValue = string.IsNullOrEmpty(method) ? "null" : method;
            builder.Append(methodValue);
        }

        builder.Append(']');
        return builder.ToString();
    }

    public void AddMethodBody(string name, string methodBody)
    {
        this.AddMethod(name, "function() { " + methodBody + "; }");
    }

    public void AddMethod(string name, string method)
    {
        this.Dictionary[name] = method;
    }

    public string ToJsonString()
    {
        return ToJsonString(this);
    }

    public string ToJavaScriptArrayString()
    {
        return ToJavaScriptArrayString(this);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        var enumerator = this.Dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<string, string>(enumerator.Key.ToString(), enumerator.Value.ToString());
        }
    }

    public override string ToString()
    {
        return this._dictionary == null ? string.Empty : this._dictionary.ToString();
    }

    private static string ToJsonString(IEnumerable<KeyValuePair<string, string>> methods)
    {
        if (methods == null)
        {
            return "null";
        }

        var builder = new StringBuilder();
        builder.Append('{');
        var isFirstPair = true;
        foreach (var keyValuePair in methods)
        {
            if (isFirstPair)
            {
                isFirstPair = false;
            }
            else
            {
                builder.Append(',');
            }

            builder.Append('"');
            builder.Append(HttpUtility.JavaScriptStringEncode(keyValuePair.Key));
            builder.Append('"');
            builder.Append(':');
            var methodValue = string.IsNullOrEmpty(keyValuePair.Value) ? "null" : keyValuePair.Value;
            builder.Append(methodValue);
        }

        builder.Append('}');
        return builder.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumeratorPrivate();
    }

    private IEnumerator GetEnumeratorPrivate()
    {
        return this.GetEnumerator();
    }
}
