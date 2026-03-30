using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TodoApi.Models;

namespace TodoApi.Services;

public class TodoItemsService
{
    private readonly IMongoCollection<TodoItem> _todoItemsCollection;

    public TodoItemsService(IOptions<TodoDatabaseSettings> todoDatabaseSettings)
    {
        var mongoClient = new MongoClient(todoDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(todoDatabaseSettings.Value.DatabaseName);

        _todoItemsCollection = mongoDatabase.GetCollection<TodoItem>(
            todoDatabaseSettings.Value.TodoItemsCollectionName);
    }

    public async Task<List<TodoItem>> GetAsync() =>
        await _todoItemsCollection.Find(_ => true).ToListAsync();

    public async Task<TodoItem?> GetAsync(string id) =>
        await _todoItemsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(TodoItem newTodoItem) =>
        await _todoItemsCollection.InsertOneAsync(newTodoItem);

    public async Task UpdateAsync(string id, TodoItem updatedTodoItem) =>
        await _todoItemsCollection.ReplaceOneAsync(x => x.Id == id, updatedTodoItem);

    public async Task RemoveAsync(string id) =>
        await _todoItemsCollection.DeleteOneAsync(x => x.Id == id);
}
