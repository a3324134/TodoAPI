using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TodoAPI.Abstracts;
using TodoAPI.Models;

namespace TodoAPI.DTOs
{
    public class TodoListPostUpDto
    {
        [ModelBinder(BinderType = typeof(FormDataJsonBinder))]
        public TodoListPostDto TodoList { get; set; }
        public IEnumerable<IFormFile> files { get; set; }
    }
}
