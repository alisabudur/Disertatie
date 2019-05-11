using System;

namespace DataAccess.Attributes
{
    /// <summary>
    /// Identifier attribute represents the "primary key" column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IdentifierAttribute : Attribute
    {
    }
}
