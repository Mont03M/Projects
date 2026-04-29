using MongoDB.Bson.Serialization.Attributes;

namespace server.apps.todoApp.DTOs;

public class TodoList
{
    public string? Name { get; set; }
    public string? Task { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? DueDate { get; set; }
}