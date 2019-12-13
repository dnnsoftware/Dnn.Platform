#region Usings

using System.Linq;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
	/// <summary>
	/// Interface of VocabularyController.
	/// </summary>
	/// <seealso cref="VocabularyController"/>
    public interface IVocabularyController
    {
        int AddVocabulary(Vocabulary vocabulary);

        void ClearVocabularyCache();

        void DeleteVocabulary(Vocabulary vocabulary);

        IQueryable<Vocabulary> GetVocabularies();

        void UpdateVocabulary(Vocabulary vocabulary);
    }
}
