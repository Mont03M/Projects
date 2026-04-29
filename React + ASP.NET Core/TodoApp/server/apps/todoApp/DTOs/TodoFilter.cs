using MongoDB.Bson.Serialization.Attributes;

namespace server.apps.todoApp.DTOs;

public class TodoFilter
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? DueDate { get; set; }
}