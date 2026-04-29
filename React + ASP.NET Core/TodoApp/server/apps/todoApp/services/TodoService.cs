using server.apps.todoApp.models;
using MongoDB.Driver;
using MongoDB.Bson;
using server.apps.todoApp.DTOs;
using System.ComponentModel;

namespace server.apps.todoApp.services;

public class TodoService
{
    private readonly IGenericService<Todo> _handler;
    private readonly CounterService _counterService;

    public TodoService(IGenericService<Todo> handler, CounterService counterService)
    {
        _handler = handler;
        _counterService = counterService;
    }

    /// <summary>
    ///  Method returns a single todo based on id
    /// </summary>
    /// <param name="id">accepts a string containing the id of the todos</param>
    /// <returns>object</returns>
    public async Task<Todo?> GetByIdAsync(string id)
    {
        return await _handler.GetByIdAsync(id);
    }

    /// <summary>
    ///  Method creates and new todo with a dynamically created taskId and name
    /// </summary>
    /// <param name="entity">Todo Model containing fields to add to new todo</param>
    /// <returns>object</returns>
    public async Task<Todo?> CreateAsync(Todo entity)
    {
        // add taskId and create name dynamically
        if (entity.TaskId == 0)
        {
            var seq = await _counterService.GetNextSequenceAsync("todo");
            entity.TaskId = seq;
            entity.Name = "TS" + seq.ToString("D4");
        }
        return await _handler.CreateAsync(entity);
    }

    /// <summary>
    ///  Method updates a todo that exists in the database and returns updated todo object
    /// </summary>
    /// <params name="id", name="entity">string id and Todo entity containing the all fields of a todo</param>
    /// <returns>object</returns>
    public async Task<Todo?> UpdateAsync(string id, Todo entity)
    {
        return await _handler.UpdateAsync(id, entity);
    }

   /// <summary>
    ///  Method deletes a single todo based on passed in string id and returns true/false if object was deleted
    /// </summary>
    /// <param name="id"> string containing the id of the todo</param>
    /// <returns>boolean</returns>
    public async Task<bool?> DeleteAsync(string id)
    {
        return await _handler.DeleteAsync(id);
    }


    /// <summary>
    ///  Method accepts a filter object to filter based on specific params
    /// </summary>
    /// <param name="filter"> Todo Fiter object DTOs</param>
    /// <returns>object(s)</returns>
    public async Task<List<Todo>> FilterAsync(TodoFilter filter)
    {
        var f = Builders<Todo>.Filter;
        var query = f.Empty;

        // check if values are null
        // add to query
        if (!string.IsNullOrWhiteSpace(filter.Status))
            query &= f.Regex(
                x => x.Status,
                new BsonRegularExpression(filter.Status, "i")
            );

        if (!string.IsNullOrWhiteSpace(filter.Priority))
            query &= f.Eq(x => x.Priority, filter.Priority);

         // check if date is empty
        // convert string date to DateTime object
        if (!string.IsNullOrWhiteSpace(filter.DueDate))
        {
            // true
            // add to query
            if (DateTime.TryParse(filter.DueDate, out var parsedDate))
            {
                var date = parsedDate.Date;

                var start = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                var end = start.AddDays(1);

                query &= f.And(
                    f.Gte(x => x.DueDate, start),
                    f.Lt(x => x.DueDate, end)
                );
            }
        }


        return await _handler.FilterAsync(query);
    }

   /// <summary>
    ///  Method accepts a TodoList object and gets the objects based on passed in values.
    /// TodoList is treated as query - building a query string.
    /// For example, list?name=T0003&status=Pending returns objects that contains these values in their fields.
    /// If the filter is empty it returns the entire list.
    /// </summary>
    /// <param name="p">Todo List object</param>
    /// <returns>filter object</returns>
    public async Task<List<Todo>> GetListAsync(TodoList p)
    {
        var f = Builders<Todo>.Filter;
        var filter = f.Empty;

        // if not empty add to filter
        // equals name add. 
        if (!string.IsNullOrWhiteSpace(p.Name))
            filter &= f.Eq(x => x.Name, p.Name);

        if (!string.IsNullOrWhiteSpace(p.Status))
            filter &= f.Eq(x => x.Status, p.Status);

        if (!string.IsNullOrWhiteSpace(p.Priority))
            filter &= f.Eq(x => x.Priority, p.Priority);

         // check if date is empty
         if (!string.IsNullOrWhiteSpace(p.DueDate))
        {
            // convert to DateTime Object and add to filter
            if (DateTime.TryParse(p.DueDate, out var parsedDate))
            {
                var date = parsedDate.Date;

                var start = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                var end = start.AddDays(1);

                filter &= f.And(
                    f.Gte(x => x.DueDate, start),
                    f.Lt(x => x.DueDate, end)
                );
            }
        }

        return await _handler.GetAllAsync(filter);
    }


    /// <summary>
    ///  Method returns todo objects based on list of keywords ["Completed", "Pending", "2026-04-21"]
    /// and matches these values to todo objects stored and find docs with these fields.
    /// </summary>
    /// <param name="keywords">List of keywords</param>
    /// <returns>query object</returns>
    public async Task<List<Todo>> SearchAsync(List<string> keywords)
    {
        var f = Builders<Todo>.Filter;
        var query = f.Empty;

        foreach (var k in keywords.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var keyword = k.Trim();

            query &= f.Or(
                f.Regex(x => x.Name, new BsonRegularExpression(keyword, "i")),
                f.Regex(x => x.Task, new BsonRegularExpression(keyword, "i")),
                f.Regex(x => x.Description, new BsonRegularExpression(keyword, "i")),
                f.Regex(x => x.Status, new BsonRegularExpression(keyword, "i")),
                f.Regex(x => x.Priority, new BsonRegularExpression(keyword, "i"))
            );

            if (DateTime.TryParse(keyword, out var date))
            {
                var d = date.Date;
                query &= f.Gte(x => x.DueDate, d) &
                         f.Lt(x => x.DueDate, d.AddDays(1));
            }
        }

        return await _handler.FilterAsync(query);
    }
}