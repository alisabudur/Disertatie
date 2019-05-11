using System;
using System.Collections.Generic;
using System.Data.OleDb;
using DataAccess.Extensions;

namespace DataAccess.Repositories
{
    /// <summary>
    /// This a generic repository for handling CRUD operations on a Excel sheet.
    /// </summary>
    /// <typeparam name="T">The type to which the Excel sheet is mapped.</typeparam>
    public class Repository<T> : IRepository<T> where T : class, new()
    {
        private readonly string _connectionString;

        /// <summary>
        /// This a generic repository for handling CRUD operations on a Excel sheet.
        /// </summary>
        /// <param name="connectionString">Represents the connection string to the 
        /// working Excel file.</param>
        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Gets all rows from Excel sheet that is mapped to the used object type.
        /// </summary>
        /// <returns>Returns a list of type Metrics</returns>
        public IEnumerable<T> GetAll()
        {
            var currentType = typeof(T);
            var selectStatement = currentType.GetSelectStatement();
            var itemsList = new List<T>();

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand command = new OleDbCommand(selectStatement, connection);
                connection.Open();
                OleDbDataReader reader = command.ExecuteReader();

                while (reader != null && reader.Read())
                {
                    var item = new T();
                    foreach (var property in currentType.GetProperties())
                    {
                        var value = reader[property.GetMappingColumn()];

                        if (value != DBNull.Value)
                            property.SetValue(item, value);
                    }

                    itemsList.Add(item);
                }

                reader?.Close();
            }

            return itemsList;
        }


        /// <summary>
        /// Gets the row from Excel sheet that is mapped to the used object type, having the gived ID.
        /// </summary>
        /// <returns>Returns a row of type Metrics</returns>
        public T GetSingle(object id)
        {
            var currentType = typeof(T);
            var selectStatement = currentType.GetSelectStatementForSingle(id);

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand command = new OleDbCommand(selectStatement, connection);
                connection.Open();
                OleDbDataReader reader = command.ExecuteReader();

                if (reader != null && reader.Read())
                {
                    var item = new T();
                    foreach (var property in currentType.GetProperties())
                    {
                        var value = reader[property.GetMappingColumn()];

                        if (value != DBNull.Value)
                            property.SetValue(item, value);
                    }

                    return item;
                }

                reader?.Close();
            }

            return null;
        }

        /// <summary>
        /// Adds a new row in the Excel sheet that is mapped to the used object type.
        /// </summary>
        /// <param name="entity">The entity holding the row values we want to add.</param>
        public void Add(T entity)
        {
            var currentType = typeof(T);
            var insertStatement = currentType.GetInsertStatement(entity);

            using (OleDbConnection connection = new
                OleDbConnection(_connectionString))
            {
                connection.Open();
                OleDbCommand command = new
                    OleDbCommand(insertStatement, connection);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Updates a row in the Excel sheet that is mapped to the used object type.
        /// </summary>
        /// <param name="entity">The entity holding the row values we want to update.</param>
        public void Update(T entity)
        {
            var currentType = typeof(T);
            var updateStatement = currentType.GetUpdateStatement(entity);

            using (OleDbConnection connection = new
                OleDbConnection(_connectionString))
            {
                connection.Open();
                OleDbCommand command = new
                    OleDbCommand(updateStatement, connection);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Delets a row in the Excel sheet that is mapped to the used object type.
        /// </summary>
        /// <param name="entiry">The entity holding the row values we want to delete.</param>
        public void Delete(T entiry)
        {
            throw new NotImplementedException();
        }
    }
}
