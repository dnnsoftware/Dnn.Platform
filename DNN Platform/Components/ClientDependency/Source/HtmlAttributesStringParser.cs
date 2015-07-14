using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Query.Dynamic;

namespace ClientDependency.Core
{
    internal static class HtmlAttributesStringParser
    {

        internal static void ParseIntoDictionary(string attributes, IDictionary<string, string> destination)
        {
            if (string.IsNullOrEmpty(attributes))
                return;
            if (destination == null)
                return;

            foreach (var parts in attributes.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries)))
            {
                if (parts.Length != 2)
                {
                    throw new ParseException("Could not parse html string attributes, the format must be key1:value1,key2:value2", parts.Length);
                }

                if (!destination.ContainsKey(parts[0]))
                    destination.Add(parts[0], parts[1]);
            }
        }
    }
}