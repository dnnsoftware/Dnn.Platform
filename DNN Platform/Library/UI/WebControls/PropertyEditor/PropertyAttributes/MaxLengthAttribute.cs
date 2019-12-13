#region Usings

using System;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MaxLengthAttribute : Attribute
    {
        private readonly int _Length = Null.NullInteger;

        /// <summary>
        /// Initializes a new instance of the MaxLengthAttribute class.
        /// </summary>
        public MaxLengthAttribute(int length)
        {
            _Length = length;
        }

        public int Length
        {
            get
            {
                return _Length;
            }
        }
    }
}
