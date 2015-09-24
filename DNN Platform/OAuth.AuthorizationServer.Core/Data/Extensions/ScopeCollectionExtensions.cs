using System.Collections.Generic;
using OAuth.AuthorizationServer.Core.Data.Model;

namespace OAuth.AuthorizationServer.Core.Data.Extensions
{
    public static class ScopeCollectionExtensions
    {
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
