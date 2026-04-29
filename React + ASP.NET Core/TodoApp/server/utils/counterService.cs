using MongoDB.Driver;
using server.utils;

public class CounterService
{
    private readonly IMongoCollection<Counter> _collection;

    public CounterService(IMongoCollection<Counter> collection)
    {
        _collection = collection;
    }

    public async Task<int> GetNextSequenceAsync(string name)
    {
        var filter = Builders<Counter>.Filter.Eq(x => x.Name, name);
        var update = Builders<Counter>.Update.Inc(x => x.Seq, 1);

        var options = new FindOneAndUpdateOptions<Counter>
        {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = true
        };

        var result = await _collection.FindOneAndUpdateAsync(filter, update, options);

        return result.Seq;
    }
}