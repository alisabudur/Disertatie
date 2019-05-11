using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DataAccess.Attributes;

namespace DataAccess.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Builds the Select all statement for the type to which this method is applied.
        /// </summary>
        /// <param name="currentType">Represents the object to which this method could be applied.</param>
        /// <returns>Returns the Select statement.</returns>
        public static string GetSelectStatement(this Type currentType)
        {
            var workSheet = currentType.GetMappingSheet();
            var selectStatement = $"SELECT * FROM [{workSheet}$]";
            return selectStatement;
        }

        /// <summary>
        /// Builds the Select by ID statement for the type to which this method is applied.
        /// </summary>
        /// <param name="currentType">Represents the object to which this method could be applied.</param>
        /// <param name="id">Id of the entity that we look for.</param>
        /// <returns>Returns the Select statement.</returns>
        public static string GetSelectStatementForSingle(this Type currentType, object id)
        {
            var workSheet = currentType.GetMappingSheet();
            var identifierProperty = currentType.GetIdentifierProperty();

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine($"SELECT TOP 1 * FROM [{workSheet}$]");
            queryBuilder.AppendLine(
                identifierProperty.PropertyType == typeof(string)
                    ? $"WHERE [{identifierProperty.GetMappingColumn()}] = '{id}'"
                    : $"WHERE [{identifierProperty.GetMappingColumn()}] = {id}");

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Builds the Insert statement for the type to which this method is applied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentType">Represents the object to which this method could be applied.</param>
        /// <param name="entity">Represents the object that should be updated.</param>
        /// <returns>Returns the Update statement.</returns>
        public static string GetInsertStatement<T>(this Type currentType, T entity) where T : class
        {
            var workSheet = currentType.GetMappingSheet();
            var listOfColumns = new List<string>();
            var listOfValues = new List<object>();

            foreach (var property in currentType.GetProperties())
            {
                listOfColumns.Add(property.GetMappingColumn());
                listOfValues.Add(property.PropertyType == typeof(string) 
                    ? $"'{property.GetValue(entity)}'"
                    : property.GetValue(entity));
            }

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine($"INSERT INTO [{workSheet}$] ({string.Join(", ", listOfColumns)})");
            queryBuilder.AppendLine($"VALUES ({string.Join(", ", listOfValues)})");

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Builds the Update statement for the type to which this method is applied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentType">Represents the object to which this method could be applied.</param>
        /// <param name="entity">Represents the object that should be updated.</param>
        /// <returns>Returns the Update statement.</returns>
        public static string GetUpdateStatement<T>(this Type currentType, T entity) where T : class
        {
            var workSheet = currentType.GetMappingSheet();
            var identifierProperty = currentType.GetIdentifierProperty();
            var setStatement = currentType.GetSetStatement(entity);

            if (identifierProperty == null)
                return string.Empty;

            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine($"UPDATE [{workSheet}$]");
            queryBuilder.AppendLine($"SET {setStatement}");

            queryBuilder.AppendLine(
                identifierProperty.PropertyType == typeof(string)
                    ? $"WHERE [{identifierProperty.GetMappingColumn()}] = '{identifierProperty.GetValue(entity)}'"
                    : $"WHERE [{identifierProperty.GetMappingColumn()}] = {identifierProperty.GetValue(entity)}");

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Gets the column to which the current property should be mapped in the Excel sheet.
        /// </summary>
        /// <param name="currentProperty">Represents the property for which we want to obtain 
        /// the mapping column.</param>
        /// <returns>Returns the column of the current property.</returns>
        public static string GetMappingColumn(this PropertyInfo currentProperty)
        {
            var propertyColumnName = currentProperty.CustomAttributes
                .FirstOrDefault(a => a.AttributeType == typeof(ColumnAttribute))
                ?.ConstructorArguments
                .FirstOrDefault()
                .Value
                .ToString();

            return propertyColumnName;
        }

        /// <summary>
        /// Decides if the currrent property is updatable or not, depending on weather 
        /// it has the NonUpdatable attribute.
        /// </summary>
        /// <param name="currentProperty">Represents the property for which we want to 
        /// decide if it is updatable or not.</param>
        /// <returns>Returns true if the property is not updatable and false otherwise. </returns>
        public static bool IsNonUpdatable(this PropertyInfo currentProperty)
        {
            return currentProperty.CustomAttributes
                .Any(a => a.AttributeType == typeof(NonUpdatableAttribute));
        }

        /// <summary>
        /// Gets the sheet to which the current class should be mapped in the Excel file.
        /// </summary>
        /// <param name="currentType">Represents the type for which we want to obtain 
        /// the mapping sheet.</param>
        /// <returns>Returns the sheet of the current type.</returns>
        public static string GetMappingSheet(this Type currentType)
        {
            var sheetName = currentType.CustomAttributes
                .FirstOrDefault(a => a.AttributeType == typeof(SheetAttribute))
                ?.ConstructorArguments
                .FirstOrDefault()
                .Value
                .ToString();

            return sheetName;
        }

        /// <summary>
        /// Gets the property which has the Identifier attribute.
        /// </summary>
        /// <param name="currentType">Represents the type for which we want to obtain the 
        /// identifier property.</param>
        /// <returns>Returns the identifier property or null if it does not exist.</returns>
        public static PropertyInfo GetIdentifierProperty(this Type currentType)
        {
            foreach (var currentProperty in currentType.GetProperties())
            {
                var isIdentifierColumn = currentProperty.CustomAttributes
                    .Any(a => a.AttributeType == typeof(IdentifierAttribute));

                if (isIdentifierColumn)
                    return currentProperty;
            }

            return null;
        }

        /// <summary>
        /// Builds the Set statement for an Update statement using type properties. It ignores 
        /// all properties that have NonUpdatable attirbute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentType">Represents the type for which we want to obtaind the Set statment.</param>
        /// <param name="item">Represents the item we want to update.</param>
        /// <returns>Returns the set statement.</returns>
        public static string GetSetStatement<T>(this Type currentType, T item) where T : class
        {
            var setList = new List<string>();

            foreach (var property in currentType.GetProperties())
            {
                if (property.IsNonUpdatable())
                    continue;

                setList.Add(property.PropertyType == typeof(string)
                    ? $"[{property.GetMappingColumn()}] = '{property.GetValue(item)}'"
                    : $"[{property.GetMappingColumn()}] = {property.GetValue(item)}");
            }

            return string.Join($",{Environment.NewLine}", setList);
        }
    }
}
