using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace server.apps.todoApp.models;

[BsonIgnoreExtraElements]
public class Todo
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("task")]
    public string? Task { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("status")]
    public string? Status { get; set; }

    [BsonElement("priority")]
    public string? Priority { get; set; }

    [BsonElement("dueDate")]
    public DateTime? DueDate { get; set; }

    [BsonElement("taskId")]
    public int TaskId { get; set; }
}