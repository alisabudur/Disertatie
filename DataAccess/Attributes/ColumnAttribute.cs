using System;

namespace DataAccess.Attributes
{
    /// <summary>
    /// Column attribute represents the Excel column to which the current property is mapped.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; }
    }
}
