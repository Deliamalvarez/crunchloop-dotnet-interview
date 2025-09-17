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
    public class TodoItemsController(TodoContext context) : ControllerBase
    {
        private readonly TodoContext _context = context;

        [HttpGet("{listId}/todoItems")]
        public async Task<ActionResult<IList<TodoItemResponse>>> GetTodoItems([FromRoute] long listId) {
            if (await IsListInvalid(listId))
            {
                return NotFound();
            }
            var items = await _context.TodoItem.Where(item => item.TodoListId == listId).ToListAsync();
            return Ok(items.Select((item) => new TodoItemResponse(item.Id, item.Title, item.Description, item.IsCompleted, item.ExternalTodoId)));
        }

        [HttpGet("{listId}/todoItems/{itemId}")]
        public async Task<ActionResult<TodoItemResponse>> GetTodoItemById([FromRoute] long listId, long itemId)
        {
            var item = await ValidateItem(listId, itemId);
            if (await IsListInvalid(listId) || item is null)
            {
                return NotFound();
            }
            return Ok(new TodoItemResponse(item.Id, item.Title, item.Description, item.IsCompleted, item.ExternalTodoId));
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

        [HttpPut("{listId}/todoItems/{itemId}")]
        public async Task<ActionResult> UpdateTodoItem([FromRoute] long listId, [FromRoute] long itemId, [FromBody] UpdateTodoItem request)
        {
            if (await IsListInvalid(listId))
            {
                return NotFound();
            }
            var item = await ValidateItem(listId, itemId);
            if (item is null)
            {
                return NotFound();
            }
            item.Title = request.Title;
            item.Description = request.Description;
            await _context.SaveChangesAsync();
            return Ok(new TodoItemResponse(item.Id, item.Title, item.Description, item.IsCompleted, item.ExternalTodoId));
        }

        [HttpPatch("{listId}/todoItems/{itemId}/complete")]
        public async Task<ActionResult> MarkTodoItemAsComplete([FromRoute] long listId, [FromRoute] long itemId)
        {
            if (await IsListInvalid(listId))
            {
                return NotFound();
            }
            var item = await ValidateItem(listId, itemId);
            if (item is null)
            {
                return NotFound();
            }
            if (item.IsCompleted)
            {
                return Conflict("Item is already completed");
            }
            item.IsCompleted = true;
            await _context.SaveChangesAsync();
            return Ok(new TodoItemResponse(item.Id, item.Title, item.Description, item.IsCompleted, item.ExternalTodoId));
        }

        [HttpDelete("{listId}/todoItems/{itemId}")]
        public async Task<ActionResult> DeleteTodoItem([FromRoute] long listId, [FromRoute] long itemId)
        {
            if (await IsListInvalid(listId))
            {
                return NotFound();
            }
            var item = await ValidateItem(listId, itemId);
            if (item is null)
            {
                return NotFound();
            }
            _context.TodoItem.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<TodoItem?> ValidateItem(long listId, long itemId)
        {
            // Please notice that I have made this cast to int because it was discussed this way during the interview call, the best approach would be to change the type of Id in the model to long
            var item = await _context.TodoItem.FindAsync((int)itemId);
            return item is not null && item.TodoListId == listId ? item : null;
        }

        private async Task<bool> IsListInvalid(long listId)
        {
            var list = await _context.TodoList.FindAsync(listId);
            return list is null;
        }

    }
}
