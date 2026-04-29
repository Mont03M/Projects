using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace server.utils;

[BsonIgnoreExtraElements]
public class Counter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("seq")]
    public int Seq { get; set; }
}