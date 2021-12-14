using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoAPI.DTOs;
using TodoAPI.Models;
using TodoAPI.Parameters;
using TodoAPI.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoAsyncController : ControllerBase
    {
        private readonly TodoListAsyncService _todoListAsyncService;
        //DI注入，可以不用再控管物件的生命週期
        public TodoAsyncController(TodoListAsyncService todoListAsyncService)
        {
            _todoListAsyncService = todoListAsyncService;
        }

        // GET: api/<TodoController>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] TodoSelectParameter value)
        {
            var result = await _todoListAsyncService.GetData(value);

            if (result == null || result.Count() <= 0)
                return NotFound("找不到資源");

            return Ok(result);
        }

        // POST api/<TodoController>
        [HttpPost]
        public async Task Post([FromBody] TodoListPostDto value)
        {
            await _todoListAsyncService.InsertData(value);

        }

    }
}
