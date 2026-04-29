using Microsoft.AspNetCore.Mvc;
using server.apps.todoApp.DTOs;
using server.apps.todoApp.models;
using server.apps.todoApp.services;

namespace TodoApi.Controllers;

[ApiController]
[Route("server/api/todos")]
public class TodoController : ControllerBase
{

    private readonly TodoService _service;

    public TodoController(TodoService service)
    {
        _service = service;
    }

    // GET server/api/todos?dueDate=2026-01-01
    [HttpGet("list")]
    public async Task<IActionResult> GetList([FromQuery] TodoList listParams)
    {
        var docs = await _service.GetListAsync(listParams);

        if (docs == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Todos not found!"
            });
        }

        return Ok(new
        {
            success = true,
            count = docs.Count,
            data = docs
        });
    }

    // POST server/api/todos/search
    [HttpPost("search")]
    public async Task<IActionResult> SearchList([FromBody] List<string> keywords)
    {
        var docs = await _service.SearchAsync(keywords);

        if (docs == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Todos not found!"
            });
        }

        return Ok(new
        {
            success = true,
            count = docs.Count,
            data = docs
        });
    }

    // POST server/api/todos/filter
    [HttpPost("filter")]
    public async Task<IActionResult> FilterList([FromBody] TodoFilter filter)
    {
        var docs = await _service.FilterAsync(filter);

        if (docs == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Todos not found!"
            });
        }

        return Ok(new
        {
            success = true,
            count = docs.Count,
            data = docs
        });
    }

    // POST server/api/todos
    [HttpPost]
    public async Task<IActionResult> CreateOne([FromBody] Todo todo)
    {
        var doc = await _service.CreateAsync(todo);

        if (doc == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Todo not created!"
            });
        }

        return Ok(new
        {
            success = true,
            data = doc
        });
    }

    // GET server/api/todos/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(string id)
    {
        var doc = await _service.GetByIdAsync(id);

        if (doc == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Todo not found"
            });
        }

        return Ok(new
        {
            success = true,
            data = doc
        });
    }

    // PATCH server/api/todos/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateOne(string id, [FromBody] Todo todo)
    {
        var doc = await _service.UpdateAsync(id, todo);

        if (doc == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Todo not found"
            });
        }

        return Ok(new
        {
            success = true,
            data = doc
        });
    }

    // DELETE server/api/todos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOne(string id)
    {
        var doc = await _service.DeleteAsync(id);

        if (doc == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Todo not deleted!"
            });
        }

        return Ok(new
        {
            success = doc,
        });
    }

    // POST server/api/todos/bulk-update
    [HttpPost("bulk-update")]
    public IActionResult BulkUpdate() => Ok();
}