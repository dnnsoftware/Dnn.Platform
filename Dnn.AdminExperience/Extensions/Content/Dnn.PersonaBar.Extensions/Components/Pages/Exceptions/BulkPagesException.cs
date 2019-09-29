using System;

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    public class BulkPagesException : Exception
    {
        public string Field { get; set; }

        public BulkPagesException(string field, string message) : base(message)
        {
            Field = field;
        }
    }
}