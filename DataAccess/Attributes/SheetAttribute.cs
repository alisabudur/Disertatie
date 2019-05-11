using System;

namespace DataAccess.Attributes
{
    /// <summary>
    /// Sheet attribute represents the Excel sheet to which the current class is mapped.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SheetAttribute : Attribute
    {
        public SheetAttribute(string sheetName)
        {
            SheetName = sheetName;
        }

        public string SheetName { get; }
    }
}
