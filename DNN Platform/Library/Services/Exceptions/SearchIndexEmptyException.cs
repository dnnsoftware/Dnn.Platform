using System;

namespace DotNetNuke.Services.Exceptions
{
    public class SearchIndexEmptyException : Exception
    {
        public SearchIndexEmptyException(string message) : base(message)
        {
        }
    }
}
