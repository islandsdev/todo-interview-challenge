// TodoItem.cs
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsImportant { get; set; }
    public DateTime CreatedDate { get; set; }
}

// ITodoService.cs
public interface ITodoService
{
    Task<IEnumerable<TodoItem>> GetAllTodosAsync();
    Task<IEnumerable<TodoItem>> GetImportantTodosAsync();
    Task<TodoItem> GetTodoByIdAsync(int id);
    Task<TodoItem> CreateTodoAsync(TodoItem todoItem);
    Task<bool> UpdateTodoAsync(TodoItem todoItem);
    Task<bool> MarkAsImportantAsync(int id);
    Task<bool> DeleteTodoAsync(int id);
}

// TodoService.cs
public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;
    private readonly ILogger<TodoService> _logger;

    public TodoService(ITodoRepository repository, ILogger<TodoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<TodoItem>> GetAllTodosAsync()
    {
        _logger.LogInformation("Getting all todos");
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetImportantTodosAsync()
    {
        _logger.LogInformation("Getting important todos");
        var todos = await _repository.GetAllAsync();
        return todos.Where(t => t.IsImportant == true);
    }

    public async Task<TodoItem> GetTodoByIdAsync(int id)
    {
        _logger.LogInformation("Getting todo with id " + id);
        return await _repository.GetByIdAsync(id);
    }

    public async Task<TodoItem> CreateTodoAsync(TodoItem todoItem)
    {
        todoItem.CreatedDate = DateTime.UtcNow;
        todoItem.IsCompleted = false;
        _logger.LogInformation("Creating new todo");
        return await _repository.AddAsync(todoItem);
    }

    public async Task<bool> UpdateTodoAsync(TodoItem todoItem)
    {
        _logger.LogInformation($"Updating todo with id {todoItem.Id}");
        return await _repository.UpdateAsync(todoItem);
    }

    public async Task<bool> MarkAsImportantAsync(int id)
    {
        var todo = await _repository.GetByIdAsync(id);
        if (todo == null)
            return false;
        todo.IsImportant = true;
        return await _repository.UpdateAsync(todo);
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        _logger.LogInformation($"Deleting todo with id {id}");
        return await _repository.DeleteAsync(id);
    }
}

// TodoController.cs
[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetAllTodos()
    {
        var todos = await _todoService.GetAllTodosAsync();
        return Ok(todos);
    }

    [HttpGet("important")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetImportantTodos()
    {
        var todos = await _todoService.GetImportantTodosAsync();
        return Ok(todos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetTodoById(int id)
    {
        var todo = await _todoService.GetTodoByIdAsync(id);
        if (todo == null)
            return NotFound();
        return Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> CreateTodo(TodoItem todoItem)
    {
        if (string.IsNullOrWhiteSpace(todoItem.Title))
            todoItem.Title = "New Task";

        var createdTodo = await _todoService.CreateTodoAsync(todoItem);
        return CreatedAtAction(nameof(GetTodoById), new { id = createdTodo.Id }, createdTodo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(int id, TodoItem todoItem)
    {
        if (id != todoItem.Id)
            return BadRequest();

        var success = await _todoService.UpdateTodoAsync(todoItem);
        if (!success)
            return NotFound();
        return NoContent();
    }

    [HttpPatch("{id}/important")]
    public async Task<IActionResult> MarkAsImportant(int id)
    {
        var result = await _todoService.MarkAsImportantAsync(id);
        if (!result)
            return NotFound();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var success = await _todoService.DeleteTodoAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
}
