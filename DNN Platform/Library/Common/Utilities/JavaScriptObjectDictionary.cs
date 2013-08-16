using System;
using System.Text;
using System.Web;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

public class JavaScriptObjectDictionary : IEnumerable<KeyValuePair<string, string>>
{
    private OrderedDictionary _dictionary = null;

    internal OrderedDictionary Dictionary
    {
        get
        {
            return _dictionary ?? (_dictionary = new OrderedDictionary());
        }
    }

    public void AddMethodBody(string name, string methodBody)
    {
        AddMethod(name, "function() { " + methodBody + "; }");
    }

    public void AddMethod(string name, string method)
    {
        Dictionary[name] = method;
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

    public string ToJsonString()
    {
        return ToJsonString(this);
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

    public string ToJavaScriptArrayString()
    {
        return ToJavaScriptArrayString(this);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        var enumerator = Dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<string, string>(enumerator.Key.ToString(), enumerator.Value.ToString());
        }
    }

    private IEnumerator GetEnumeratorPrivate()
    {
        return GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumeratorPrivate();
    }

    public override string ToString()
    {
        return _dictionary == null ? string.Empty : _dictionary.ToString();
    }
}
