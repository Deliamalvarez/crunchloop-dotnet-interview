using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Controllers;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Tests.Controllers;

public class TodoItemsControllerTests
{
    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoList.Add(new TodoList { Id = 1, Name = "Task 1" });
        context.TodoItem.Add(new TodoItem { Id = 1, Title = "Todo 1", Description = "bla bla bla", IsCompleted = false, TodoListId = 1 });
        context.TodoItem.Add(new TodoItem { Id = 2, Title = "Todo 2", Description = "bla bla bla", IsCompleted = false, TodoListId = 1 });
        context.SaveChanges();
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    public async Task GetTodoItemById_WhenCalled_ReturnsTodoItemAsExpected(long listId, int itemId)
    {
        // Arrange
        await using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);
        var controller = new TodoItemsController(context);
        //Act
        var actionResult = await controller.GetTodoItemById(listId, itemId);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var todoItem = Assert.IsAssignableFrom<TodoItemResponse>(okResult.Value);
        var expectedItem = context.TodoItem.Find(itemId);
        Assert.NotNull(expectedItem);
        Assert.NotNull(todoItem);
        Assert.Equal(itemId, todoItem.Id);
        Assert.Equal(expectedItem.Title, todoItem.Title);
        Assert.Equal(expectedItem.Description, todoItem.Description);
        Assert.Equal(expectedItem.IsCompleted, todoItem.IsCompleted);
    }

    [Theory]
    [InlineData(999, 2)]
    [InlineData(1, 999)]
    public async Task GetTodoItemById_WhenCalled_WithWrongIds_ReturnsNotFoundResponse(long listId, long itemId)
    {
        // Arrange
        await using var context = new TodoContext(DatabaseContextOptions());
        PopulateDatabaseContext(context);
        var controller = new TodoItemsController(context);
        //Act
        var actionResult = await controller.GetTodoItemById(listId, itemId);
        // Assert
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }
}
