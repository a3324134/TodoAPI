using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TodoAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly TodoContext _todoContext;
        private readonly IWebHostEnvironment _env;

        public FileUploadController(IWebHostEnvironment env, TodoContext todoContext)
        {
            _env = env;
            _todoContext = todoContext;
        }
        // POST api/<FileUploadController>
        [HttpPost("{id}")]
        public void Post(IEnumerable<IFormFile> files, Guid id)
        {
            string rootRoot = _env.ContentRootPath + @"\wwwroot\UploadFiles\" + id + "\\";

            if (!Directory.Exists(rootRoot))
                Directory.CreateDirectory(rootRoot);
            foreach(var file in files)
            {
                string fileName = file.FileName;
                using (var stream = System.IO.File.Create(rootRoot + fileName))
                {
                    file.CopyTo(stream);

                    var insert = new UploadFile
                    {
                        Name = fileName,
                        Src = "/UploadFiles/" + id + "/" + fileName,
                        TodoId = id
                    };

                    _todoContext.UploadFiles.Add(insert);
                }
            }
            _todoContext.SaveChanges();
        }
      
    }
}
