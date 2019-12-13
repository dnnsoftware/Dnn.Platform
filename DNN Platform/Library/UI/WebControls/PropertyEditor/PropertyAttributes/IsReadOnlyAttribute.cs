#region Usings

using System;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IsReadOnlyAttribute : Attribute
    {
        private readonly bool _IsReadOnly;

        /// <summary>
        /// Initializes a new instance of the ReadOnlyAttribute class.
        /// </summary>
        /// <param name="read">A boolean that indicates whether the property is ReadOnly</param>
        public IsReadOnlyAttribute(bool read)
        {
            _IsReadOnly = read;
        }

        public bool IsReadOnly
        {
            get
            {
                return _IsReadOnly;
            }
        }
    }
}
