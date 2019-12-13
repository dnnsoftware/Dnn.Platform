using System;

namespace DotNetNuke.ComponentModel.DataAnnotations
{
    public class ScopeAttribute : Attribute
    {

        public ScopeAttribute(string scope)
        {
            Scope = scope;
        }

        /// <summary>
        /// The property to use to scope the cache.  The default is an empty string.
        /// </summary>
        public string Scope { get; set; }

    }
}
