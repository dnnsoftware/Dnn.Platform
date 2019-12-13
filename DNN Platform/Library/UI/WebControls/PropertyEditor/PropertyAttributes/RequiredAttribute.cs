#region Usings

using System;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredAttribute : Attribute
    {
        private readonly bool _Required;

        /// <summary>
        /// Initializes a new instance of the RequiredAttribute class.
        /// </summary>
        public RequiredAttribute(bool required)
        {
            _Required = required;
        }

        public bool Required
        {
            get
            {
                return _Required;
            }
        }
    }
}
