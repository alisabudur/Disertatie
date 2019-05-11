using System.Collections.Generic;

namespace DataAccess.Repositories
{
    /// <summary>
    /// This a generic repository interface for handling CRUD operations on a Excel sheet.
    /// </summary>
    /// <typeparam name="T">The type to which the Excel sheet is mapped.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all rows from Excel sheet that is mapped to the used object type.
        /// </summary>
        /// <returns>Returns a list of type Metrics</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Gets the row from Excel sheet that is mapped to the used object type, having the gived ID.
        /// </summary>
        /// <returns>Returns a row of type Metrics</returns>
        T GetSingle(object id);

        /// <summary>
        /// Adds a new row in the Excel sheet that is mapped to the used object type.
        /// </summary>
        /// <param name="entity">The entity holding the row values we want to add.</param>
        void Add(T entity);

        /// <summary>
        /// Updates a row in the Excel sheet that is mapped to the used object type.
        /// </summary>
        /// <param name="entity">The entity holding the row values we want to update.</param>
        void Update(T entity);

        /// <summary>
        /// Delets a row in the Excel sheet that is mapped to the used object type.
        /// </summary>
        /// <param name="entiry">The entity holding the row values we want to delete.</param>
        void Delete(T entiry);
    }
}
