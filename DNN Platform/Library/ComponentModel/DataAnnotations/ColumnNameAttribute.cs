#region Usings

using System;

#endregion

namespace DotNetNuke.ComponentModel.DataAnnotations
{
    public class ColumnNameAttribute : Attribute
    {
        public ColumnNameAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; set; }
    }
}
