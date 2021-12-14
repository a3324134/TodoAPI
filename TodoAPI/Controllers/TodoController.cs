using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoAPI.DTOs;
using TodoAPI.Models;
using TodoAPI.Parameters;
using Microsoft.AspNetCore.JsonPatch;
using System.Text.Json;
using TodoAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        //private readonly IWebHostEnvironment _env;
        private readonly TodoContext _todoContext;
        private readonly TodoListService _todoListService;
        //DI注入，可以不用再控管物件的生命週期
        public TodoController(TodoContext todoContext, TodoListService todoListService, IWebHostEnvironment env)
        {
            //_env = env;
            _todoContext = todoContext;
            _todoListService = todoListService;
        }
        
        // GET: api/<TodoController>
        [HttpGet]
        [Authorize(Roles = "select")]
        public IActionResult Get([FromQuery]TodoSelectParameter value)
        {
            var result = _todoListService.GetData(value);

            if (result == null || result.Count() <= 0)
                return NotFound("找不到資源");

            return Ok(result);
        }

        // GET api/<TodoController>/5
        [HttpGet("{id}")]
        [Authorize(Roles = "select")]
        public ActionResult<TodoListDto> GetOne(Guid id)
        {
            var result = _todoListService.GetDataByTodoId(id);

            if (result == null)
                return NotFound("找不到id: " + id + "的資料");

            return result;
         }

        // POST api/<TodoController>
        [HttpPost]
        [Authorize(Roles = "insert")]
        public IActionResult Post([FromBody] TodoListPostDto value)
        {
            var insert = _todoListService.InsertData(value);

            return CreatedAtAction(nameof(GetOne), new { id = insert.TodoId }, insert);
        }

        [HttpPost("withupload")]
        public IActionResult PostWithUpload([FromForm] TodoListPostUpDto value)
        {
            var insert = _todoListService.InsertDataWithUpload(value);

            return CreatedAtAction(nameof(GetOne), new { id = insert.TodoId }, insert);
        }


        [HttpPost("PostSQL")]
        public IActionResult PostSQL([FromBody] TodoListPostDto value)
        {
            var insert = _todoListService.InsertDataBySQL(value);
            if (insert >= 1)
                return Ok();
            else
                return BadRequest("新增資料錯誤");
        }

        // PUT api/<TodoController>/5
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] TodoListPutDto value)
        {
            if (id != value.TodoId)
                return BadRequest();

            if (_todoListService.UpdateData(id, value) == 0)
                return NotFound();

            return NoContent();            
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(Guid id, [FromBody] JsonPatchDocument value)
        {

            if(_todoListService.UpdateData(id, value) == 0)
                return NotFound();

            return NoContent();
        }

        // DELETE api/<TodoController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            if (_todoListService.DeleteData(id) == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("list/{ids}")]
        public IActionResult Delete(string ids)
        {
            if(_todoListService.DeleteBulkData(ids) == 0)
                return NotFound();

            return NoContent();
        }

        
    }
}
