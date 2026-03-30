namespace TodoApi.Models;

public class TodoItemDTO
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}