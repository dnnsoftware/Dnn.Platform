using System;
namespace DotNetNuke.Collections
{
    /// <summary>
    /// This interface used to make a class can have index declaration.
    /// </summary>
    internal interface IIndexable
    {
        object this[string name] { get; set; }
    }
}
