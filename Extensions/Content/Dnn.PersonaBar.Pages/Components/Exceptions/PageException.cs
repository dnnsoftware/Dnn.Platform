using System;

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    public class PageException : Exception
    {
        public PageException(string message): base(message)
        {
        }
    }
}