using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/todolist")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemsController(TodoContext context)
        {
                _context = context;
        }
        // api/{listId}/todoitems
        [HttpGet("{listId}/todoItems")]
        public async Task<ActionResult<IList<TodoItemResponse>>> GetTodoItems([FromRoute] long listId) {
            if (await IsListInvalid(listId))
            {
                return NotFound();
            }
            var items = await _context.TodoItem.Where(item => item.TodoListId == listId).ToListAsync();
            return Ok(items.Select((item) => new TodoItemResponse(item.Id, item.Title, item.Description)));
        }

        [HttpPost("{listId}/todoItems")]
        public async Task<ActionResult> CreateTodoItem([FromRoute] long listId, [FromBody] CreateTodoItem request)
        {
            if (await IsListInvalid(listId))
            {
                return NotFound();
            }

            _context.TodoItem.Add(new TodoItem { Title = request.Title, Description = request.Description, TodoListId = listId});
            await _context.SaveChangesAsync();

            return Created();
        }

        private async Task<bool> IsListInvalid(long listId)
        {
            var list = await _context.TodoList.FindAsync(listId);
            return list is null;
        }

    }
}
