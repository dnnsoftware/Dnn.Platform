#region Usings

using System;

#endregion

namespace DotNetNuke.ComponentModel.DataAnnotations
{
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute(string columnName) : this(columnName, columnName)
        {
        }

        public PrimaryKeyAttribute(string columnName, string propertyName)
        {
            ColumnName = columnName;
            PropertyName = propertyName;
            AutoIncrement = true;
        }

        public bool AutoIncrement { get; set; }
        public string ColumnName { get; set; }
        public string PropertyName { get; set; }

    }
}
