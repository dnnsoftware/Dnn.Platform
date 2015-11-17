using System.Collections.Generic;
using OAuth.AuthorizationServer.Core.Data.Model;

namespace OAuth.AuthorizationServer.Core.Data.Extensions
{
    /// <summary>
    /// extend the scope collection to return identifier
    /// </summary>
    public static class ScopeCollectionExtensions
    {
        /// <summary>
        /// return scope identifier
        /// </summary>
        public static HashSet<string> ToScopeIdentifierSet(this ICollection<Scope> scopes)
        {
            var scopeSet = new HashSet<string>();
            foreach (var s in scopes)
            {
                scopeSet.Add(s.Identifier);
            }
            return scopeSet;
        }
    }
}
