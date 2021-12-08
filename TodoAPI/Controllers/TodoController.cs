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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _todoContext;
        
        //DI注入，可以不用再控管物件的生命週期
        public TodoController(TodoContext todoContext)
        {
            _todoContext = todoContext;
        }
        
        // GET: api/<TodoController>
        [HttpGet]
        public IActionResult Get([FromQuery]TodoSelectParameter value)
        {
            var result = _todoContext.TodoLists
                .Include(a => a.UpdateEmployee)
                .Include(a => a.InsertEmployee)
                .Include(a => a.UploadFiles)
                .Select(a => a);

            if (!string.IsNullOrWhiteSpace(value.name))
                result = result.Where(a => a.Name.Contains(value.name));

            if (value.enable != null)
                result = result.Where(a => a.Enable == value.enable);

            if (value.insertTime != null)
                result = result.Where(a => a.InsertTime.Date == value.insertTime);

            if (value.minOrder != null && value.maxOrder != null)
                result = result.Where(a => a.Orders >= value.minOrder && a.Orders <= value.maxOrder);

            if (result == null || result.Count() <= 0)
                return NotFound("找不到資源");


            return Ok(result.ToList().Select(a => ItemToDto(a)));
        }

        // GET api/<TodoController>/5
        [HttpGet("{id}")]
        public ActionResult<TodoListDto> GetOne(Guid id)
        {
            var result = (from todo in _todoContext.TodoLists
                          where todo.TodoId == id
                          select new TodoListDto
                          {
                              Enable = todo.Enable,
                              InsertEmployeeName = todo.InsertEmployee.Name,
                              InsertTime = todo.InsertTime,
                              Name = todo.Name,
                              Orders = todo.Orders,
                              TodoId = todo.TodoId,
                              UpdateEmployeeName = todo.UpdateEmployee.Name,
                              UpdateTime = todo.UpdateTime,
                              UploadFiles = (from file in _todoContext.UploadFiles
                                             where file.TodoId == todo.TodoId
                                             select new UploadFileDto
                                             {
                                                 Name = file.Name,
                                                 Src = file.Src,
                                                 TodoId = file.TodoId,
                                                 UploadFileId = file.UploadFileId
                                             }).ToList()
                          }).SingleOrDefault();

            if (result == null)
                return NotFound("找不到id: " + id + "的資料");

            return result;
         }

        // POST api/<TodoController>
        [HttpPost]
        public IActionResult Post([FromBody] TodoListPostDto value)
        {
            TodoList insert = new TodoList
            {               
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            };

            _todoContext.TodoLists.Add(insert).CurrentValues.SetValues(value);
            _todoContext.SaveChanges();

            foreach (var file in value.UploadFiles)
            {
                _todoContext.UploadFiles.Add(new UploadFile()
                {
                    TodoId = insert.TodoId
                }).CurrentValues.SetValues(file);
            }

            _todoContext.SaveChanges();

            return CreatedAtAction(nameof(GetOne), new { id = insert.TodoId }, insert);
        }

        [HttpPost("PostSQL")]
        public void PostSQL([FromBody] TodoListPostDto value)
        {
            var name = new SqlParameter("name", value.Name);


            string sql = @"INSERT INTO [dbo].[TodoList]
           ([Name]
           ,[InsertTime]
           ,[UpdateTime]
           ,[Enable]
           ,[Orders]
           ,[InsertEmployeeId]
           ,[UpdateEmployeeId])
     VALUES
           (@name,'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" +
           DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + value.Enable + "','" +
           value.Orders + "','00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000001')";

            _todoContext.Database.ExecuteSqlRaw(sql, name);

        }

        // PUT api/<TodoController>/5
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] TodoListPutDto value)
        {
            if (id != value.TodoId)
                return BadRequest();

            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();

            if (update != null)
            {

                update.UpdateTime = DateTime.Now;
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                _todoContext.TodoLists.Update(update).CurrentValues.SetValues(value);
                _todoContext.SaveChanges();
            }
            else
                return NotFound();

            return NoContent();

            
        }


        [HttpPatch("{id}")]
        public IActionResult Patch(Guid id, [FromBody] JsonPatchDocument value)
        {

            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();

            if (update != null)
            {
                update.UpdateTime = DateTime.Now;
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                value.ApplyTo(update);
                _todoContext.SaveChanges();
            }
            else
                return NotFound();

            return NoContent();
        }

        // DELETE api/<TodoController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var delete = (from todo in _todoContext.TodoLists
                          where todo.TodoId == id
                          select todo).Include(item => item.UploadFiles).SingleOrDefault();

            if (delete != null)
            {
                _todoContext.TodoLists.Remove(delete);
                _todoContext.SaveChanges();
            }
            else
                return NotFound();

            return NoContent();
        }

        [HttpDelete("list/{ids}")]
        public IActionResult Delete(string ids)
        {
            var deleteList = JsonSerializer.Deserialize<List<Guid>>(ids);

            var delete = (from todo in _todoContext.TodoLists
                          where deleteList.Contains(todo.TodoId)
                          select todo).Include(item => item.UploadFiles);

            if (delete != null)
            {
                _todoContext.TodoLists.RemoveRange(delete);
                _todoContext.SaveChanges();
            }
            else
                return NotFound();

            return NoContent();
        }

        private static TodoListDto ItemToDto(TodoList item)
        {
            List<UploadFileDto> uploadDto = new List<UploadFileDto>();
            foreach(var tmp in item.UploadFiles)
            {
                UploadFileDto file = new UploadFileDto
                {
                    Name = tmp.Name,
                    Src = tmp.Src,
                    TodoId = tmp.TodoId,
                    UploadFileId = tmp.UploadFileId
                };
                uploadDto.Add(file);
            }
            return new TodoListDto
            {
                Enable = item.Enable,
                InsertEmployeeName = item.InsertEmployee.Name,
                InsertTime = item.InsertTime,
                Name = item.Name,
                Orders = item.Orders,
                TodoId = item.TodoId,
                UpdateEmployeeName = item.UpdateEmployee.Name,
                UpdateTime = item.UpdateTime,
                UploadFiles = uploadDto
            };
        }
    }
}
