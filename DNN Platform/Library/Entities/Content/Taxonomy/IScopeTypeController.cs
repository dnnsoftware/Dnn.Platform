#region Usings

using System.Linq;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
	/// <summary>
	/// Interface of ScopeTypeController.
	/// </summary>
	/// <seealso cref="ScopeTypeController"/>
    public interface IScopeTypeController
    {
        int AddScopeType(ScopeType scopeType);

        void ClearScopeTypeCache();

        void DeleteScopeType(ScopeType scopeType);

        IQueryable<ScopeType> GetScopeTypes();

        void UpdateScopeType(ScopeType scopeType);
    }
}
