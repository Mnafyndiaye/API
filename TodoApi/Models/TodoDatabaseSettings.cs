namespace TodoApi.Models;

public class TodoDatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string TodoItemsCollectionName { get; set; } = string.Empty;
}
