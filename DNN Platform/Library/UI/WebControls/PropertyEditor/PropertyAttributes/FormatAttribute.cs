#region Usings

using System;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatAttribute : Attribute
    {
        private readonly string _Format;

        /// <summary>
        /// Initializes a new instance of the FormatAttribute class.
        /// </summary>
        public FormatAttribute(string format)
        {
            _Format = format;
        }

        public string Format
        {
            get
            {
                return _Format;
            }
        }
    }
}
