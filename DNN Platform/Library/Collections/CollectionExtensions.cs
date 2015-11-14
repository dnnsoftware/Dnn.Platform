﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Collections
{
    public static class CollectionExtensions
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CollectionExtensions));
        /// <summary>
        /// This method is extracted from the FriendlyUrlSettings class
        /// </summary>
        public static Dictionary<String, String> CreateDictionaryFromString(string stringOfPairs, char pairsSeparator, char pairSeparator)
        {
            var dictionary = new Dictionary<String, String>();
            if (!string.IsNullOrEmpty(stringOfPairs))
            {
                var pairs = stringOfPairs.Split(pairsSeparator);
                foreach (var pair in pairs)
                {
                    var keyValues = pair.Split(pairSeparator);
                    string key = null;
                    string value = null;
                    if (keyValues.GetUpperBound(0) >= 0)
                    {
                        key = keyValues[0];
                    }
                    if (keyValues.GetUpperBound(0) >= 1)
                    {
                        value = keyValues[1];
                    }
                    if (!string.IsNullOrEmpty(key) && value != null && !dictionary.ContainsKey(key))
                    {
                        dictionary.Add(key, value);
                    }
                }
            }
            return dictionary;
        }

        public static string DictionaryToString(this Dictionary<String, String> dictionary, string pairsSeparator, string pairSeparator)
        {
            return String.Join(pairsSeparator, dictionary.Select(pair => pair.Key + pairSeparator + pair.Value));
        }

        /// <summary>
        /// Gets a converter function which parses a <see cref="string"/> value into a <see cref="bool"/>.
        /// Considers the value <c>true</c> if it is one of the following (case-insensitive):
        /// <list type="bullet">
        /// <item><term>true</term></item>
        /// <item><term>on</term></item>
        /// <item><term>1</term></item>
        /// <item><term>yes</term></item>
        /// </list>
        /// </summary>
        /// <returns>A <see cref="Func{String,Boolean}" /> instance.</returns>
        public static Func<string, bool> GetFlexibleBooleanParsingFunction()
        {
            return GetFlexibleBooleanParsingFunction(new[] { "true", "on", "1", "yes" });
        }

        /// <summary>Gets a converter function which parses a <see cref="string"/> value into a <see cref="bool"/>.</summary>
        /// <param name="trueValues">The <see cref="string"/> values (case-insensitive) which should be parsed as <c>true</c>.</param>
        /// <returns>A <see cref="Func{String,Boolean}" /> instance.</returns>
        public static Func<string, bool> GetFlexibleBooleanParsingFunction(params string[] trueValues)
        {
            return value => trueValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        #region GetValue Extension Methods

        /// <summary>Gets the value from the dictionary.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="dictionary"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this IDictionary dictionary, string key)
        {
            return dictionary.GetValue(key, ConvertValue<T>);
        }

        /// <summary>Gets the value from the lookup.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        /// <exception cref="ArgumentException"><paramref name="lookup"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this ILookup<string, string> lookup, string key)
        {
            return lookup.ToDictionary(key).GetValue<T>(key);
        }

        /// <summary>Gets the value from the XML node's child elements, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="node"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this IXPathNavigable node, string key)
        {
            return node.ToDictionary().GetValue<T>(key);
        }

        /// <summary>Gets the value from the collection.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        /// <exception cref="ArgumentException"><paramref name="collection"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this NameValueCollection collection, string key)
        {
            return collection.ToLookup(false).GetValue<T>(key);
        }

        /// <summary>Gets the value from the XML node's child elements.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="node"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this XContainer node, string key)
        {
            return node.ToDictionary().GetValue<T>(key);
        }

        /// <summary>Gets the value from the dictionary.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> or <paramref name="converter"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="dictionary"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this IDictionary dictionary, string key, Func<object, T> converter)
        {
            Requires.NotNull("dictionary", dictionary);
            Requires.NotNull("converter", converter);

            if (!dictionary.Contains(key))
            {
                throw new ArgumentException("dictionary does not contain a value for the given key", "key");
            }

            return converter(dictionary[key]);
        }

        /// <summary>Gets the value from the lookup.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        /// <exception cref="ArgumentException"><paramref name="lookup"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this ILookup<string, string> lookup, string key, Func<object, T> converter)
        {
            return lookup.ToDictionary(key).GetValue(key, converter);
        }

        /// <summary>Gets the value from the XML node's child elements.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="node"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this IXPathNavigable node, string key, Func<object, T> converter)
        {
            return node.ToDictionary().GetValue(key, converter);
        }

        /// <summary>Gets the value from the collection.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        /// <exception cref="ArgumentException"><paramref name="collection"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this NameValueCollection collection, string key, Func<object, T> converter)
        {
            return collection.ToLookup(false).GetValue(key, converter);
        }

        /// <summary>Gets the value from the XML node's child elements.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="node"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this XContainer node, string key, Func<object, T> converter)
        {
            return node.ToDictionary().GetValue(key, converter);
        }

        /// <summary>Gets the value from the dictionary.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="dictionary"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this IDictionary dictionary, string key, Func<string, T> converter)
        {
            return dictionary.GetValue(key, (object value) => ConvertValue(value, converter));
        }

        /// <summary>Gets the value from the lookup.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        /// <exception cref="ArgumentException"><paramref name="lookup"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this ILookup<string, string> lookup, string key, Func<string, T> converter)
        {
            return lookup.ToDictionary(key).GetValue(key, converter);
        }

        /// <summary>Gets the value from the XML node's child elements.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="node"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this IXPathNavigable node, string key, Func<string, T> converter)
        {
            return node.ToDictionary().GetValue(key, converter);
        }

        /// <summary>Gets the value from the collection.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        /// <exception cref="ArgumentException"><paramref name="collection"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this NameValueCollection collection, string key, Func<string, T> converter)
        {
            return collection.ToLookup(false).GetValue(key, converter);
        }

        /// <summary>Gets the value from the XML node's child elements.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="node"/> does not contain a value for <paramref name="key"/></exception>
        public static T GetValue<T>(this XContainer node, string key, Func<string, T> converter)
        {
            return node.ToDictionary().GetValue(key, converter);
        }

        #endregion

        #region GetValueOrDefault Extension Methods

        /// <summary>Gets the value from the dictionary, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IDictionary dictionary, string key)
        {
            return dictionary.GetValueOrDefault(key, default(T));
        }

        /// <summary>Gets the value from the lookup, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this ILookup<string, string> lookup, string key)
        {
            return lookup.ToDictionary(key).GetValueOrDefault<T>(key);
        }

        /// <summary>Gets the value from the XML node's child elements, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IXPathNavigable node, string key)
        {
            return node.ToDictionary().GetValueOrDefault<T>(key);
        }

        /// <summary>Gets the value from the collection, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this NameValueCollection collection, string key)
        {
            return collection.ToLookup(false).GetValueOrDefault<T>(key);
        }

        /// <summary>Gets the value from the XML node's child elements, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this XContainer node, string key)
        {
            return node.ToDictionary().GetValueOrDefault<T>(key);
        }

        /// <summary>Gets the value from the dictionary, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the dictionary doesn't have a value for the given <paramref name="key"/>.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IDictionary dictionary, string key, T defaultValue)
        {
            return dictionary.GetValueOrDefault(key, defaultValue, ConvertValue<T>);
        }

        /// <summary>Gets the value from the lookup, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the lookup doesn't have a value for the given <paramref name="key"/>.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this ILookup<string, string> lookup, string key, T defaultValue)
        {
            return lookup.ToDictionary(key).GetValueOrDefault(key, defaultValue);
        }

        /// <summary>Gets the value from the XML node's child elements, returning <paramref name="defaultValue"/>  if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the node doesn't have a value for the given <paramref name="key"/>.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IXPathNavigable node, string key, T defaultValue)
        {
            return node.ToDictionary().GetValueOrDefault(key, defaultValue);
        }

        /// <summary>Gets the value from the collection, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the collection doesn't have a value for the given <paramref name="key"/>.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this NameValueCollection collection, string key, T defaultValue)
        {
            return collection.ToLookup(false).GetValueOrDefault(key, defaultValue);
        }

        /// <summary>Gets the value from the XML node's child elements, returning <paramref name="defaultValue"/>  if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the node doesn't have a value for the given <paramref name="key"/>.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this XContainer node, string key, T defaultValue)
        {
            return node.ToDictionary().GetValueOrDefault(key, defaultValue);
        }

        /// <summary>Gets the value from the dictionary, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IDictionary dictionary, string key, Func<object, T> converter)
        {
            return dictionary.GetValueOrDefault(key, default(T), converter);
        }

        /// <summary>Gets the value from the lookup, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this ILookup<string, string> lookup, string key, Func<object, T> converter)
        {
            return lookup.ToDictionary(key).GetValueOrDefault(key, converter);
        }

        /// <summary>Gets the value from the XML node's child elements, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IXPathNavigable node, string key, Func<object, T> converter)
        {
            return node.ToDictionary().GetValueOrDefault(key, converter);
        }

        /// <summary>Gets the value from the collection, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this NameValueCollection collection, string key, Func<object, T> converter)
        {
            return collection.ToLookup(false).GetValueOrDefault(key, converter);
        }

        /// <summary>Gets the value from the XML node's child elements, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this XContainer node, string key, Func<object, T> converter)
        {
            return node.ToDictionary().GetValueOrDefault(key, converter);
        }

        /// <summary>Gets the value from the dictionary, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IDictionary dictionary, string key, Func<string, T> converter)
        {
            return dictionary.GetValueOrDefault(key, default(T), (object value) => ConvertValue(value, converter));
        }

        /// <summary>Gets the value from the lookup, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this ILookup<string, string> lookup, string key, Func<string, T> converter)
        {
            return lookup.ToDictionary(key).GetValueOrDefault(key, converter);
        }

        /// <summary>Gets the value from the XML node's child elements, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IXPathNavigable node, string key, Func<string, T> converter)
        {
            return node.ToDictionary().GetValueOrDefault(key, converter);
        }

        /// <summary>Gets the value from the collection, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this NameValueCollection collection, string key, Func<string, T> converter)
        {
            return collection.ToLookup(false).GetValueOrDefault(key, converter);
        }

        /// <summary>Gets the value from the XML node's child elements, returning the default value of <typeparamref key="T" /> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this XContainer node, string key, Func<string, T> converter)
        {
            return node.ToDictionary().GetValueOrDefault(key, converter);
        }

        /// <summary>Gets the value from the dictionary, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the dictionary doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IDictionary dictionary, string key, T defaultValue, Func<string, T> converter)
        {
            return dictionary.GetValueOrDefault(key, defaultValue, (object value) => ConvertValue(value, converter));
        }

        /// <summary>Gets the value from the lookup, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the lookup doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this ILookup<string, string> lookup, string key, T defaultValue, Func<string, T> converter)
        {
            return lookup.ToDictionary(key).GetValueOrDefault(key, defaultValue, converter);
        }

        /// <summary>Gets the value from the XML node's child elements, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the node doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as a <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IXPathNavigable node, string key, T defaultValue, Func<string, T> converter)
        {
            return node.ToDictionary().GetValueOrDefault(key, defaultValue, converter);
        }

        /// <summary>Gets the value from the collection, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the collection doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as an <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this NameValueCollection collection, string key, T defaultValue, Func<string, T> converter)
        {
            return collection.ToLookup(false).GetValueOrDefault(key, defaultValue, converter);
        }

        /// <summary>Gets the value from the XML node's child elements, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the node doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as a <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this XContainer node, string key, T defaultValue, Func<string, T> converter)
        {
            return node.ToDictionary().GetValueOrDefault(key, defaultValue, converter);
        }

        /// <summary>Gets the value from the lookup, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the lookup doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="lookup"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this ILookup<string, string> lookup, string key, T defaultValue, Func<object, T> converter)
        {
            return lookup.ToDictionary(key).GetValueOrDefault(key, defaultValue, converter);
        }

        /// <summary>Gets the value from the XML node's child elements, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the node doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IXPathNavigable node, string key, T defaultValue, Func<object, T> converter)
        {
            return node.ToDictionary().GetValueOrDefault(key, defaultValue, converter);
        }

        /// <summary>Gets the value from the collection, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the collection doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException"><paramref name="collection"/> has multiple values for the given <paramref name="key"/></exception>
        public static T GetValueOrDefault<T>(this NameValueCollection collection, string key, T defaultValue, Func<object, T> converter)
        {
            return collection.ToLookup(false).GetValueOrDefault(key, defaultValue, converter);
        }

        /// <summary>Gets the value from the XML node's child elements, returning <paramref name="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="node">An XML node which containers other elements.</param>
        /// <param name="key">The name of the element from which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the node doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this XContainer node, string key, T defaultValue, Func<object, T> converter)
        {
            return node.ToDictionary().GetValueOrDefault(key, defaultValue, converter);
        }

        /// <summary>Gets the value from the dictionary, returning the <paramref key="defaultValue"/> if the value doesn't exist.</summary>
        /// <typeparam name="T">The type of the value to retrieve</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key by which to get the value.</param>
        /// <param name="defaultValue">The default value to return if the dictionary doesn't have a value for the given <paramref name="key"/>.</param>
        /// <param name="converter">A function to convert the value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> or <paramref name="converter"/> is <c>null</c></exception>
        public static T GetValueOrDefault<T>(this IDictionary dictionary, string key, T defaultValue, Func<object, T> converter)
        {
            Requires.NotNull("dictionary", dictionary);
            Requires.NotNull("converter", converter);
            var value = defaultValue;
            try
            {
                value = dictionary.Contains(key) ? converter(dictionary[key]) : defaultValue;
            }
            catch (Exception)
            {
                Logger.ErrorFormat("Error loading portal setting: {0} Default value {1} was used instead", key + ":" + dictionary[key], defaultValue.ToString());
            }
            return value;
        }

        #endregion

        #region GetValues Extension Methods

        /// <summary>Gets the values from the lookup.</summary>
        /// <typeparam name="T">The type of the values to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the values.</param>
        /// <returns>A sequence of <typeparamref name="T"/> instances.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> is <c>null</c></exception>
        public static IEnumerable<T> GetValues<T>(this ILookup<string, string> lookup, string key)
        {
            return lookup.GetValues(key, ConvertValue<T>);
        }

        /// <summary>Gets the values from the collection.</summary>
        /// <typeparam name="T">The type of the values to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the values.</param>
        /// <returns>A sequence of <typeparamref name="T"/> instances.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        public static IEnumerable<T> GetValues<T>(this NameValueCollection collection, string key)
        {
            return collection.ToLookup().GetValues<T>(key);
        }

        /// <summary>Gets the values from the lookup.</summary>
        /// <typeparam name="T">The type of the values to retrieve</typeparam>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key by which to get the values.</param>
        /// <param name="converter">A function to convert a value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A sequence of <typeparamref name="T"/> instances.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup"/> or <paramref name="converter"/> is <c>null</c></exception>
        public static IEnumerable<T> GetValues<T>(this ILookup<string, string> lookup, string key, Func<string, T> converter)
        {
            Requires.NotNull("lookup", lookup);
            Requires.NotNull("converter", converter);

            return lookup[key].Select(converter);
        }

        /// <summary>Gets the values from the collection.</summary>
        /// <typeparam name="T">The type of the values to retrieve</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="key">The key by which to get the values.</param>
        /// <param name="converter">A function to convert a value as an <see cref="object"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A sequence of <typeparamref name="T"/> instances.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="converter"/> is <c>null</c></exception>
        public static IEnumerable<T> GetValues<T>(this NameValueCollection collection, string key, Func<string, T> converter)
        {
            return collection.ToLookup().GetValues(key, converter);
        }

        #endregion

        /// <summary>Converts the <paramref name="collection"/> to an <see cref="ILookup{TKey,TElement}"/>.</summary>
        /// <param name="collection">The collection.</param>
        /// <returns>An <see cref="ILookup{TKey,TElement}"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <c>null</c></exception>
        public static ILookup<string, string> ToLookup(this NameValueCollection collection)
        {
            return collection.ToLookup(true);
        }

        /// <summary>Converts the <paramref name="collection" /> to an <see cref="ILookup{TKey,TElement}" />.</summary>
        /// <param name="collection">The collection.</param>
        /// <param name="splitValues">If <c>true</c>, treats values in the <paramref name="collection"/> as comma-delimited lists of items (e.g. from a <see cref="NameValueCollection"/>)</param>
        /// <returns>An <see cref="ILookup{TKey,TElement}" /> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection" /> is <c>null</c></exception>
        public static ILookup<string, string> ToLookup(this NameValueCollection collection, bool splitValues)
        {
            Requires.NotNull("collection", collection);
            return collection.AllKeys
                             .SelectMany(key => ParseValues(key, collection.GetValues(key), splitValues))
                             .ToLookup(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Executes an action for each element in the source collection.
        /// </summary>
        /// <typeparam name="TType">The type of the type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static IEnumerable<TType> ForEach<TType>(this IEnumerable<TType> source, Action<TType> action)
        {
            foreach (TType element in source)
            {
                action(element);
            }

            return source;
        }

        #region Private Methods

        /// <summary>Converts the <paramref name="value"/> into a <typeparamref name="T"/> instance.</summary>
        /// <typeparam name="T">The type of the value to return</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        /// <exception cref="InvalidCastException">
        /// the value is <c>null</c> and <typeparamref name="T"/> is a value type, or
        /// the value does not implement the <see cref="IConvertible"/> interface and
        /// no cast is defined from the value to <typeparamref name="T"/>
        /// </exception>
        private static T ConvertValue<T>(object value)
        {
            if (value is T)
            {
                return (T)value;
            }

            if (value is IConvertible)
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }

            if (value == null)
            {
                if (typeof(T).IsValueType)
                {
                    if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return (T)value;
                    }

                    // TODO: this should probably return the default value if called from GetOrDefault...
                    throw new InvalidCastException();
                }

                return (T)value; // null
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)value.ToString();
            }

            return (T)value;
        }

        /// <summary>Converts the <paramref name="value" /> into a <typeparamref name="T" /> instance.</summary>
        /// <typeparam name="T">The type of the value to return</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="converter">A function to convert a <see cref="string"/> to a <typeparamref name="T"/> instance.</param>
        /// <returns>A <typeparamref name="T" /> instance.</returns>
        private static T ConvertValue<T>(object value, Func<string, T> converter)
        {
            var formattable = value as IFormattable;
            return converter(value == null ? null : formattable == null ? value.ToString() : formattable.ToString(null, CultureInfo.InvariantCulture));
        }

        /// <summary>Wraps the <paramref name="values"/> into <see cref="KeyValuePair{TKey,TValue}"/> instances.</summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>A sequence of <see cref="KeyValuePair{TKey,TValue}"/> instances.</returns>
        private static IEnumerable<KeyValuePair<string, string>> ParseValues(string key, string[] values)
        {
            return ParseValues(key, values, true);
        }

        /// <summary>Wraps the <paramref name="values"/> into <see cref="KeyValuePair{TKey,TValue}"/> instances.</summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <param name="splitSingleValue">If <c>true</c>, treats a single item in <paramref name="values"/> as a comma-delimited list of items (e.g. from a <see cref="NameValueCollection"/>)</param>
        /// <returns>A sequence of <see cref="KeyValuePair{TKey,TValue}"/> instances.</returns>
        private static IEnumerable<KeyValuePair<string, string>> ParseValues(string key, string[] values, bool splitSingleValue)
        {
            return (splitSingleValue && values.Length == 1
                        ? values[0].Split(',')
                        : values).Select(value => new KeyValuePair<string, string>(key, value));
        }

        /// <summary>Converts the <paramref name="node"/> to a <see cref="Dictionary{TKey,TValue}"/>.</summary>
        /// <param name="node">The node.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        private static Dictionary<string, string> ToDictionary(this XContainer node)
        {
            Requires.NotNull("node", node);
            return node.Descendants().ToDictionary(n => n.Name.ToString(), n => n.Value);
        }

        /// <summary>Converts the <paramref name="node"/> to a <see cref="Dictionary{TKey,TValue}"/>.</summary>
        /// <param name="node">The node.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c></exception>
        private static Dictionary<string, string> ToDictionary(this IXPathNavigable node)
        {
            Requires.NotNull("node", node);
            return node.CreateNavigator().SelectChildren(XPathNodeType.Element).Cast<XPathNavigator>().ToDictionary(n => n.Name, n => n.Value);
        }

        /// <summary>Converts the <paramref name="lookup" /> to a <see cref="Dictionary{TKey,TValue}" /> for the specific <paramref name="key" />.</summary>
        /// <param name="lookup">The lookup.</param>
        /// <param name="key">The key.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}" /> instance with zero or one key/value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lookup" /> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException">There were multiple values for the given key</exception>
        private static Dictionary<string, string> ToDictionary(this ILookup<string, string> lookup, string key)
        {
            Requires.NotNull("lookup", lookup);
            try
            {
                return lookup[key].ToDictionary(v => key);
            }
            catch (ArgumentException exc)
            {
                // TODO: Localize this
                throw new InvalidOperationException("There were multiple values for the given key", exc);
            }
        }

        #endregion
    }

}
