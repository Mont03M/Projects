using MongoDB.Driver;
using server.apps.todoApp.models;
using server.utils;

namespace server.apps.contexts;

public class MongoContext
{
    private readonly IMongoDatabase _db;

    public MongoContext(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        _db = client.GetDatabase(config["MongoDB:Database"]);
    }

    public IMongoCollection<Todo> Todos =>
        _db.GetCollection<Todo>("todos");

    public IMongoCollection<Counter> Counters =>
    _db.GetCollection<Counter>("counters");
}