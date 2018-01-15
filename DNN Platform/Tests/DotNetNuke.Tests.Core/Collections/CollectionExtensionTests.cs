#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;

using DotNetNuke.Collections;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class CollectionExtensionTests : AssertionHelper
    {
        [Test]
        public void get_null_string_from_hashtable_for_missing_value()
        {
            var table = new Hashtable { { "app id", "abc123" } };

            var value = table.GetValueOrDefault<string>("cat id");

            Expect(value, Is.Null);
        }

        [Test]
        public void can_get_string_from_hashtable()
        {
            var table = new Hashtable { { "app id", "abc123" } };

            var value = table.GetValueOrDefault<string>("app id");

            Expect(value, Is.EqualTo("abc123"));
        }

        [Test]
        public void get_string_from_hashtable_when_default_is_provided()
        {
            var table = new Hashtable { { "app id", "abc123" } };

            var value = table.GetValueOrDefault("app id", "abracadabra");

            Expect(value, Is.EqualTo("abc123"));
        }

        [Test]
        public void can_get_default_string_from_hashtable()
        {
            var table = new Hashtable { { "app id", "abc123" } };

            var value = table.GetValueOrDefault("cat id", "Frank");

            Expect(value, Is.EqualTo("Frank"));
        }

        [Test]
        public void can_get_bool_from_hashtable()
        {
            var table = new Hashtable { { "app id", "true" } };

            var value = table.GetValueOrDefault<bool>("app id");

            Expect(value, Is.True);
        }

        [Test]
        public void get_bool_from_hashtable_when_default_is_provided()
        {
            var table = new Hashtable { { "app id", "true" } };

            var value = table.GetValueOrDefault("app id", false);

            Expect(value, Is.True);
        }

        [Test]
        public void can_get_default_bool_from_hashtable()
        {
            var value = true;
            var table = new Hashtable { { "app id", "abc123" } };

            value = table.GetValueOrDefault("Allow Windows Live Writer", value);

            Expect(value, Is.True);
        }

        [Test]
        public void get_false_from_hashtable_for_missing_value()
        {
            var table = new Hashtable { { "app id", "abc123" } };

            var value = table.GetValueOrDefault<bool>("Allow Windows Live Writer");

            Expect(value, Is.False);
        }

        [Test]
        public void get_bool_with_custom_converter_from_hashtable()
        {
            var table = new Hashtable { { "allow", "on" } };

            var value = table.GetValueOrDefault(
                "allow",
                v =>
                {
                    bool allowed;
                    if (!bool.TryParse(v, out allowed))
                    {
                        allowed = v.Equals("on", StringComparison.Ordinal);
                    }

                    return allowed;
                });

            Expect(value, Is.True);
        }

        [Test]
        public void get_int()
        {
            var collection = new Dictionary<string, string> { { "appId", "123" } };

            var value = collection.GetValueOrDefault<int>("appId");

            Expect(value, Is.EqualTo(123));
        }

        [Test]
        public void get_decimal()
        {
            var collection = new Dictionary<string, string> { { "appId", "1.23" } };

            var value = collection.GetValueOrDefault<decimal>("appId");

            Expect(value, Is.EqualTo(1.23m));
        }

        [Test]
        [SetCulture("nl-NL")]
        public void get_decimal_from_other_culture()
        {
            var collection = new Dictionary<string, string> { { "appId", "1.23" } };

            var value = collection.GetValueOrDefault<decimal>("appId");

            Expect(value, Is.EqualTo(1.23m));
        }

        [Test]
        public void get_datetime()
        {
            var collection = new Dictionary<string, string> { { "startDate", "05/04/2012 00:00:00" } };

            var value = collection.GetValueOrDefault<DateTime>("startDate");

            Expect(value, Is.EqualTo(new DateTime(2012, 5, 4)));
        }

        [Test]
        public void get_null_string_without_default()
        {
            var collection = new Dictionary<string, string> { { "app id", null } };

            var value = collection.GetValue<string>("app id");

            Expect(value, Is.Null);
        }

        [Test]
        public void get_null_string_with_default()
        {
            var collection = new Dictionary<string, string> { { "app id", null } };

            var value = collection.GetValueOrDefault("app id", "a default value");

            Expect(value, Is.Null);
        }

        [Test]
        public void get_nullable_datetime()
        {
            var collection = new Dictionary<string, DateTime?> { { "startDate", null } };

            var value = collection.GetValue<DateTime?>("startDate");

            Expect(value, Is.Null);
        }

        [Test]
        [SetCulture("nl-NL")]
        public void get_datetime_from_other_culture()
        {
            var collection = new Dictionary<string, string> { { "startDate", "05/04/2012 00:00:00" } };

            var value = collection.GetValueOrDefault<DateTime>("startDate");

            Expect(value, Is.EqualTo(new DateTime(2012, 5, 4)));
        }

        [Test]
        public void get_from_statebag()
        {
            var collection = new StateBag { { "appId", "123" } };

            var value = collection.GetValueOrDefault<string>("appId");

            Expect(value, Is.EqualTo("123"));
        }

        [Test]
        public void get_from_xnode()
        {
            var node = new XElement(
                "parent",
                new XElement("id", 14));

            var value = node.GetValueOrDefault<int>("id");

            Expect(value, Is.EqualTo(14));
        }

        [Test]
        public void get_from_xmlnode()
        {
            var doc = new XmlDocument();
            doc.LoadXml(@"
<parent>
    <id>13</id>
</parent>");

            var value = doc.DocumentElement.GetValueOrDefault<int>("id");

            Expect(value, Is.EqualTo(13));
        }

        [Test]
        public void can_get_timespan_with_custom_converter()
        {
            var collection = new Hashtable { { "length", "1:10:10" } };

            var value = collection.GetValueOrDefault("length", TimeSpan.Parse);

            Expect(value, Is.EqualTo(TimeSpan.FromSeconds(4210)));
        }

        [Test]
        public void can_get_empty_boolean_from_form()
        {
            var collection = new NameValueCollection { { "text", "blah" } };

            var value = collection.GetValueOrDefault("radio", CollectionExtensions.GetFlexibleBooleanParsingFunction());

            Expect(value, Is.False);
        }

        [Test]
        public void can_get_boolean_from_form()
        {
            var collection = new NameValueCollection { { "radio", "on" } };

            var value = collection.GetValueOrDefault("radio", CollectionExtensions.GetFlexibleBooleanParsingFunction());

            Expect(value, Is.True);
        }

        [Test]
        public void flexible_boolean_parsing_is_case_insensitive()
        {
            var collection = new NameValueCollection { { "question", "YES" } };

            var value = collection.GetValueOrDefault("question", CollectionExtensions.GetFlexibleBooleanParsingFunction("yes"));

            Expect(value, Is.True);
        }

        [Test]
        public void can_convert_namevaluecollection_to_lookup()
        {
            var collection = new NameValueCollection { { "question", "YES" } };

            var lookup = collection.ToLookup();

            Expect(lookup["question"], Is.EquivalentTo(new[] { "YES" }));
        }

        [Test]
        public void can_convert_namevaluecollection_with_multiple_values_to_lookup()
        {
            var collection = new NameValueCollection { { "question", "A" }, { "question", "B" }, { "question", "C" }, };

            var lookup = collection.ToLookup();

            Expect(lookup["question"], Is.EquivalentTo(new[] { "A", "B", "C", }));
        }

        [Test]
        public void can_get_null_value_rather_than_default()
        {
            var dictionary = new Hashtable { { "question", null } };

            var value = dictionary.GetValueOrDefault("question", "yes");

            Expect(value, Is.Null);
        }

        [Test]
        public void can_get_empty_string_rather_than_default()
        {
            var dictionary = new Hashtable { { "question", string.Empty } };

            var value = dictionary.GetValueOrDefault("question", "yes");

            Expect(value, Is.Empty);
        }

        [Test]
        public void can_get_value_without_default()
        {
            var dictionary = new Hashtable { { "question", "what is it" } };

            var value = dictionary.GetValue<string>("question");

            Expect(value, Is.EqualTo("what is it"));
        }

        [Test]
        public void can_get_value_without_default_with_custom_converter()
        {
            var dictionary = new Hashtable { { "question", "what is it" } };

            var value = dictionary.GetValue("question", (object v) => v is string ? 10 : 20);

            Expect(value, Is.EqualTo(10));
        }

        [Test]
        public void can_get_value_without_default_from_namevaluecollection()
        {
            var collection = new NameValueCollection { { "question", "what is it" } };

            var value = collection.GetValue<string>("question");

            Expect(value, Is.EqualTo("what is it"));
        }

        [Test]
        public void can_get_value_containing_comma_from_namevaluecollection()
        {
            var collection = new NameValueCollection { { "question", "what, is it?" } };

            var value = collection.GetValue<string>("question");

            Expect(value, Is.EqualTo("what, is it?"));
        }

        [Test]
        public void can_get_multiple_values_from_namevaluecollection()
        {
            var collection = new NameValueCollection { { "state", "CA" }, { "state", "BC" }, };

            var value = collection.GetValues<string>("state");

            Expect(value, Is.EquivalentTo(new[] { "CA", "BC", }));
        }

        [Test]
        public void can_get_sequence_with_single_value_from_namevaluecollection()
        {
            var collection = new NameValueCollection { { "state", "CA" } };

            var value = collection.GetValues<string>("state");

            Expect(value, Is.EquivalentTo(new[] { "CA" }));
        }

        [Test]
        public void can_get_sequence_with_no_value_from_namevaluecollection()
        {
            var collection = new NameValueCollection { { "state", "CA" } };

            var value = collection.GetValues<string>("cat");

            Expect(value, Is.Empty);
        }

        [Test]
        public void can_get_multiple_values_from_namevaluecollection_with_custom_converter()
        {
            var collection = new NameValueCollection { { "state", "12" }, { "state", "1" } };

            var value = collection.GetValues("state", v => int.Parse(v, CultureInfo.InvariantCulture) + 10);

            Expect(value, Is.EquivalentTo(new[] { 22, 11 }));
        }

        [Test]
        public void can_get_value_without_default_from_statebag()
        {
            var dictionary = new StateBag { { "question", "what is it" } };

            var value = dictionary.GetValue<string>("question");

            Expect(value, Is.EqualTo("what is it"));
        }

        [Test]
        public void can_get_value_without_default_from_xnode()
        {
            var node = new XElement(
                "parent",
                new XElement("id", 21));

            var value = node.GetValue<int>("id");

            Expect(value, Is.EqualTo(21));
        }

        [Test]
        public void can_get_value_without_default_from_xmlnode()
        {
            var doc = new XmlDocument();
            doc.LoadXml(@"
<parent>
    <id>123</id>
</parent>");

            var value = doc.DocumentElement.GetValue<int>("id");

            Expect(value, Is.EqualTo(123));
        }

        [Test]
        public void getvalue_throws_argumentexception_when_value_is_not_present()
        {
            var dictionary = new Hashtable { { "question", "what is it" } };

            Expect(() => dictionary.GetValue<string>("answer"), Throws.ArgumentException.With.Property("ParamName").EqualTo("key"));
        }

        [Test]
        public void throws_invalidoperationexception_when_lookup_has_multiple_values()
        {
            var collection = new NameValueCollection { { "state", "CA" }, { "state", "BC" } };

            Expect(() => collection.GetValueOrDefault<string>("state"), Throws.InvalidOperationException);
        }

        [Test]
        public void throws_argumentnullexception_when_dictionary_is_null()
        {
            IDictionary dictionary = null;

            Expect(() => dictionary.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("dictionary"));
        }

        [Test]
        public void throws_argumentnullexception_when_xelement_is_null()
        {
            XElement node = null;

            Expect(() => node.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("node"));
        }

        [Test]
        public void throws_argumentnullexception_when_xmlnode_is_null()
        {
            XmlNode node = null;

            Expect(() => node.GetValueOrDefault<int>("value ID"), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("node"));
        }

        [Test]
        public void does_not_throw_invalidcastexception_when_value_is_null_for_reference_type()
        {
            var dictionary = new Dictionary<string, string> { { "length", null } };

            var value = dictionary.GetValueOrDefault<ApplicationException>("length");

            Expect(value, Is.Null);
        }

        [Test]
        public void tolookup_throws_argumentnullexception_when_namevaluecollection_is_null()
        {
            NameValueCollection col = null;

            Expect(() => col.ToLookup(), Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("collection"));
        }
    }
}
