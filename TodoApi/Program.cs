using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
// Add services to the container.
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var todoGroup = app.MapGroup("/api/todos").WithTags("Todos");

#region In-Memory Endpoints ...

// var todos = new List<TodoGetDto>
// {
//     new (1, "Learn C#", true),
//     new (2, "Learn ASP.NET Core", false),
//     new (3, "Build a web API", false)
// };

// todoGroup.MapGet("/", () => Results.Ok(todos));

// todoGroup.MapGet("/{id}", (int id) =>
// {
//     var todo = todos.FirstOrDefault(t => t.Id == id);

//     return todo is not null ? Results.Ok(todo) : Results.NotFound();    
// });

// todoGroup.MapPost("/", (TodoPostDto dto) =>
// {
//    var nextId = todos.Count == 0 ? 1 : todos.Max(t => t.Id) + 1;

//    var todo = new TodoGetDto(nextId, dto.Title, false);
//    todos.Add(todo);

//    return Results.Created($"/api/todos/{todo.Id}", todo); 
// });

// todoGroup.MapPut("/{id}", (int id, TodoUpdateDto dto) =>
// {
//  try
//     {
//         var index = todos.FindIndex(x => x.Id == id);
//         if (index == -1) return Results.NotFound();

//         todos[index] = todos[index] with
//         {
//             Title = dto.Title,
//             IsCompleted = dto.IsCompleted
//         };

//         return Results.Ok(todos[index]);
//     }
//     catch (Exception ex)
//     {
//         return Results.Problem(ex.Message);
//     }

// });

// todoGroup.MapDelete("/{id}", (int id) =>
// {
//    try
//     {
//         var todo = todos.FirstOrDefault(t => t.Id == id);
//         if (todo is null) return Results.NotFound();

//         todos.Remove(todo);
//         return Results.NoContent();
//     }
//     catch(Exception ex)
//     {
//         return Results.Problem(ex.Message);
//     }
// });

#endregion

#region Database Endpoints ...

todoGroup.MapGet("/", async (AppDBContext db) =>
{
    var todos = await db.ToDoitems.ToListAsync();

    return todos.Count == 0 ? Results.NotFound() : Results.Ok(todos);
});

todoGroup.MapPost("/", async (AppDBContext db, ToDoitem dto) =>
{
    var lastTodo = await db.ToDoitems.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
    var nextId = lastTodo is null ? 1 : lastTodo.Id + 1;
    var todo = new ToDoitem
    {
        Title = dto.Title,
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow
    };

    db.ToDoitems.Add(todo);
    await db.SaveChangesAsync();

    var todoGetDto = new TodoGetDto(todo.Id, todo.Title, todo.IsCompleted);

    return Results.Created($"/api/todos/{todo.Id}", todo);
});

#endregion
app.Run();

