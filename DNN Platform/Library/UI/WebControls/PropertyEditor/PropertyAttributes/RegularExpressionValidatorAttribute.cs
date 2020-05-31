// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RegularExpressionValidatorAttribute : Attribute
    {
        private readonly string _Expression;

        /// <summary>
        /// Initializes a new instance of the RegularExpressionValidatorAttribute class.
        /// </summary>
        public RegularExpressionValidatorAttribute(string expression)
        {
            _Expression = expression;
        }

        public string Expression
        {
            get
            {
                return _Expression;
            }
        }
    }
}
