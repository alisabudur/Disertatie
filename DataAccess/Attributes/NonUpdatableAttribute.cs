using System;

namespace DataAccess.Attributes
{
    /// <summary>
    /// Non Updatable attribute can be used for recordset fields. Such fields 
    /// correspond to cells in the Excel worksheet containing Excel formulas 
    /// (beginning with "="), are read-only and cannot be edited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NonUpdatableAttribute : Attribute
    {
    }
}
