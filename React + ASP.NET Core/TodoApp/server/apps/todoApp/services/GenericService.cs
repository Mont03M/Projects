using MongoDB.Driver;
using MongoDB.Bson;

namespace server.apps.todoApp.services;

public class GenericService<T> : IGenericService<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public GenericService(IMongoCollection<T> collection)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _collection
         .Find(Builders<T>.Filter.Eq("_id", new ObjectId(id)))
         .FirstOrDefaultAsync();
    }

    public async Task<List<T>> GetAllAsync(FilterDefinition<T> listParms)
    {
        return await _collection.Find(listParms).ToListAsync();
    }

    public async Task<List<T>> FilterAsync(FilterDefinition<T> filter)
    {
        return await _collection.Find(filter).ToListAsync();

    }

    public async Task<T?> UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
        var result = await _collection.ReplaceOneAsync(filter, entity);

        if (result.MatchedCount == 0)
            return null;

        return await _collection
            .Find(filter)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
        var result = await _collection.DeleteOneAsync(filter);

        return result.DeletedCount > 0;
    }
}