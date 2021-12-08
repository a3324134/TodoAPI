using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoAPI.DTOs;
using TodoAPI.Models;

namespace TodoAPI.Controllers
{
    [Route("api/Todo/{TodoId}/UploadFile")]
    [ApiController]
    public class TodoUploadFlieController : ControllerBase
    {
        private readonly TodoContext _todoContext;

        public TodoUploadFlieController(TodoContext todoContext)
        {
            _todoContext = todoContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UploadFileDto>> Get(Guid TodoId)
        {
            if (!_todoContext.TodoLists.Any(a => a.TodoId == TodoId))
                return NotFound("找不到該事項");

            var result = from file in _todoContext.UploadFiles
                         where file.TodoId == TodoId
                         select new UploadFileDto
                         {
                             Name = file.Name,
                             Src = file.Src,
                             TodoId = file.TodoId,
                             UploadFileId = file.UploadFileId
                         };

            if (result == null || result.Count() == 0)
                return NotFound("找不到檔案");

            return Ok(result);
        }

        [HttpGet("{UploadFileId}")]
        public ActionResult<UploadFileDto> Get(Guid TodoId, Guid UploadFileId)
        {
            if (!_todoContext.TodoLists.Any(a => a.TodoId == TodoId))
                return NotFound("找不到該事項");

            var result = (from file in _todoContext.UploadFiles
                         where file.TodoId == TodoId
                         && file.UploadFileId == UploadFileId
                         select new UploadFileDto
                         {
                             Name = file.Name,
                             Src = file.Src,
                             TodoId = file.TodoId,
                             UploadFileId = file.UploadFileId
                         }).SingleOrDefault();
            
            if (result == null)
                return NotFound("找不到檔案");
            
            return result;
        }

        [HttpPost]
        public string Post(Guid TodoId, [FromBody] UploadFilePostDto value)
        {
            if (!_todoContext.TodoLists.Any(a => a.TodoId == TodoId))
                return "找不到該事項";

            UploadFile insert = new UploadFile
            {
                Name = value.Name,
                Src = value.Src,
                TodoId = TodoId
            };

            _todoContext.UploadFiles.Add(insert);
            _todoContext.SaveChanges();
            return "OK";
        }
    }
}
