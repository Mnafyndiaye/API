using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TodoItemsController : ControllerBase
{
    private readonly TodoItemsService _todoItemsService;

    public TodoItemsController(TodoItemsService todoItemsService)
    {
        _todoItemsService = todoItemsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
    {
        return (await _todoItemsService.GetAsync())
            .Select(x => ItemToDTO(x))
            .ToList();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemDTO>> GetTodoItem(string id)
    {
        var todoItem = await _todoItemsService.GetAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        return ItemToDTO(todoItem);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> PutTodoItem(string id, TodoItemDTO todoItemDTO)
    {
        if (id != todoItemDTO.Id)
        {
            return BadRequest();
        }

        var todoItem = await _todoItemsService.GetAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        todoItem.Name = todoItemDTO.Name;
        todoItem.IsComplete = todoItemDTO.IsComplete;

        await _todoItemsService.UpdateAsync(id, todoItem);

        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<TodoItemDTO>> PostTodoItem(TodoItemDTO todoItemDTO)
    {
        var todoItem = new TodoItem
        {
            IsComplete = todoItemDTO.IsComplete,
            Name = todoItemDTO.Name
        };

        await _todoItemsService.CreateAsync(todoItem);

        return CreatedAtAction(
            nameof(GetTodoItem),
            new { id = todoItem.Id },
            ItemToDTO(todoItem));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteTodoItem(string id)
    {
        var todoItem = await _todoItemsService.GetAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        await _todoItemsService.RemoveAsync(id);

        return NoContent();
    }

    private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
        new TodoItemDTO
        {
            Id = todoItem.Id,
            Name = todoItem.Name,
            IsComplete = todoItem.IsComplete
        };
}