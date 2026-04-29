using MongoDB.Driver;
namespace server.apps.todoApp.services;

public interface IGenericService<T>
{
    Task<T> CreateAsync(T entity);
    Task<T?> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync(FilterDefinition<T> listParms);
    Task<List<T>> FilterAsync(FilterDefinition<T> filter);
    Task<T?> UpdateAsync(string id, T entity);
    Task<bool> DeleteAsync(string id);
}

