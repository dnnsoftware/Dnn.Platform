using Dnn.PersonaBar.Vocabularies.Components;

namespace Dnn.PersonaBar.Vocabularies.Exceptions
{
    public class VocabularyNameAlreadyExistsException : VocabulariesException
    {
        public VocabularyNameAlreadyExistsException()
            : base(Constants.VocabularyExistsError)
        {
            
        }
    }
}
