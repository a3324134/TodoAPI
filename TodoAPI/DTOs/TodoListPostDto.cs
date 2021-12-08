﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TodoAPI.DTOs
{
    public class TodoListPostDto
    { 
        public string Name { get; set; }
        public bool Enable { get; set; }
        public int Orders { get; set; }
        public ICollection<UploadFilePostDto> UploadFiles { get; set; }

    }
}
